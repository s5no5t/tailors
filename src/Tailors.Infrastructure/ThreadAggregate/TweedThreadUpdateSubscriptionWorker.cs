using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Raven.Client.Documents.Subscriptions;
using Raven.Client.Exceptions.Database;
using Raven.Client.Exceptions.Documents.Subscriptions;
using Raven.Client.Exceptions.Security;
using Tailors.Domain.ThreadAggregate;
using Tailors.Domain.TweedAggregate;
using Tailors.Infrastructure.TweedAggregate;

namespace Tailors.Infrastructure.ThreadAggregate;

public class TweedThreadUpdateSubscriptionWorker(IDocumentStore store,
        ILogger<TweedThreadUpdateSubscriptionWorker> logger)
    : BackgroundService
{
    private const string SubscriptionName = "TweedThreadUpdateSubscription";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation($"Starting worker for subscription {SubscriptionName}");
        await EnsureSubscriptionExists(stoppingToken);

        while (true)
        {
            SubscriptionWorkerOptions options = new(SubscriptionName)
            {
                MaxDocsPerBatch = 20
            };
            var subscriptionWorker =
                store.Subscriptions.GetSubscriptionWorker<Tweed>(options);

            try
            {
                // here we are able to be informed of any exception that happens during processing                    
                subscriptionWorker.OnSubscriptionConnectionRetry += exception =>
                {
                    logger.LogError(exception, $"Error during subscription processing: ${SubscriptionName}");
                };

                await subscriptionWorker.Run(async batch =>
                {
                    logger.LogInformation($"Processing batch of {batch.Items.Count} items");
                    using var session = batch.OpenAsyncSession();
                    foreach (var item in batch.Items)
                        await ProcessTweed(item.Result, session);
                    await session.SaveChangesAsync(stoppingToken);
                    logger.LogInformation("Finished processing batch");
                }, stoppingToken);

                return;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failure in subscription: {SubscriptionName}", SubscriptionName);

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
                logger.LogInformation(
                    $"Stopping worker {subscriptionWorker.WorkerId} for subscription {SubscriptionName}");
                await subscriptionWorker.DisposeAsync();
            }
        }
    }

    private async Task EnsureSubscriptionExists(CancellationToken stoppingToken)
    {
        try
        {
            await store.Subscriptions.GetSubscriptionStateAsync(SubscriptionName, null,
                stoppingToken);
        }
        catch (SubscriptionDoesNotExistException)
        {
            SubscriptionCreationOptions<Tweed> options = new()
            {
                Name = SubscriptionName
            };
            await store.Subscriptions.CreateAsync(options, token: stoppingToken);
        }
    }

    private async Task ProcessTweed(Tweed tweed, IAsyncDocumentSession session)
    {
        ThreadRepository threadRepository = new(session);
        TweedRepository tweedRepository = new(session);
        ThreadUseCase threadUseCase = new(threadRepository, tweedRepository);

        var result = await threadUseCase.AddTweedToThread(tweed.Id!);
        result.Switch(
            _ => { },
            error => { logger.LogError(error.Message); });
    }
}
