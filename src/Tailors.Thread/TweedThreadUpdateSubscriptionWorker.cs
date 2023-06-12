using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Raven.Client.Documents.Subscriptions;
using Raven.Client.Exceptions.Database;
using Raven.Client.Exceptions.Documents.Subscriptions;
using Raven.Client.Exceptions.Security;
using Tailors.Thread.Domain;
using Tailors.Thread.Infrastructure;

namespace Tailors.Thread;

public class TweedThreadUpdateSubscriptionWorker : BackgroundService
{
    private const string SubscriptionName = "TweedThreadUpdateSubscription";
    private readonly ILogger<TweedThreadUpdateSubscriptionWorker> _logger;
    private readonly IDocumentStore _store;

    public TweedThreadUpdateSubscriptionWorker(IDocumentStore store,
        ILogger<TweedThreadUpdateSubscriptionWorker> logger)
    {
        _store = store;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Starting worker for subscription {SubscriptionName}");
        await EnsureSubScriptionExists(stoppingToken);

        while (true)
        {
            SubscriptionWorkerOptions options = new(SubscriptionName)
            {
                MaxDocsPerBatch = 20
            };
            var subscriptionWorker =
                _store.Subscriptions.GetSubscriptionWorker<Tweed>(options);

            try
            {
                // here we are able to be informed of any exception that happens during processing                    
                subscriptionWorker.OnSubscriptionConnectionRetry += exception =>
                {
                    _logger.LogError(
                        "Error during subscription processing: " + SubscriptionName, exception);
                };

                await subscriptionWorker.Run(async batch =>
                {
                    _logger.LogInformation($"Processing batch of {batch.Items.Count} items");
                    using var session = batch.OpenAsyncSession();
                    foreach (var item in batch.Items)
                        await ProcessTweed(item.Result, session);
                    await session.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Finished processing batch");
                }, stoppingToken);

                return;
            }
            catch (Exception e)
            {
                _logger.LogError("Failure in subscription: " + SubscriptionName, e);

                if (e is DatabaseDoesNotExistException ||
                    e is SubscriptionDoesNotExistException ||
                    e is SubscriptionInvalidStateException ||
                    e is AuthorizationException)
                    throw; // not recoverable

                if (e is SubscriptionClosedException)
                    // closed explicitly by admin, probably
                    return;

                if (e is SubscriberErrorException se)
                {
                    if (se.InnerException != null) throw;

                    continue;
                }

                // handle this depending on subscription
                // open strategy (discussed later)
                if (e is SubscriptionInUseException)
                    continue;

                return;
            }
            finally
            {
                _logger.LogInformation(
                    $"Stopping worker {subscriptionWorker.WorkerId} for subscription {SubscriptionName}");
                await subscriptionWorker.DisposeAsync();
            }
        }
    }

    private async Task EnsureSubScriptionExists(CancellationToken stoppingToken)
    {
        try
        {
            await _store.Subscriptions.GetSubscriptionStateAsync(SubscriptionName, null,
                stoppingToken);
        }
        catch (SubscriptionDoesNotExistException)
        {
            SubscriptionCreationOptions<Tweed> options = new()
            {
                Name = SubscriptionName
            };
            await _store.Subscriptions.CreateAsync(options, token: stoppingToken);
        }
    }

    private async Task ProcessTweed(Tweed tweed, IAsyncDocumentSession session)
    {
        TweedThreadRepository threadRepository = new(session);
        TweedRepository tweedRepository = new(session);
        ThreadOfTweedsUseCase threadOfTweedsUseCase = new(threadRepository, tweedRepository);

        var result = await threadOfTweedsUseCase.AddTweedToThread(tweed.Id!);
        result.Match<OneOf<Success, ReferenceNotFoundError>>(
            success => success,
            error =>
            {
                _logger.LogError(error.Message);
                return error;
            });
    }
}
