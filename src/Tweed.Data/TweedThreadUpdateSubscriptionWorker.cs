using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Raven.Client.Documents.Subscriptions;
using Raven.Client.Exceptions.Database;
using Raven.Client.Exceptions.Documents.Subscriptions;
using Raven.Client.Exceptions.Security;

namespace Tweed.Data;

public class TweedThreadUpdateSubscriptionWorker : BackgroundService
{
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
        SubscriptionCreationOptions options = new()
        {
            Name = "TweedThreadUpdateSubscription"
        };
        var subscriptionName = await _store.Subscriptions.CreateAsync<Model.Tweed>(
            t => !ReferenceEquals(t.ParentTweedId, null), options, token: stoppingToken);

        while (true)
        {
            var subscriptionWorker =
                _store.Subscriptions.GetSubscriptionWorker<Model.Tweed>(subscriptionName);

            try
            {
                // here we are able to be informed of any exception that happens during processing                    
                subscriptionWorker.OnSubscriptionConnectionRetry += exception =>
                {
                    _logger.LogError(
                        "Error during subscription processing: " + subscriptionName, exception);
                };

                await subscriptionWorker.Run(async batch =>
                {
                    using var session = batch.OpenAsyncSession();
                    // TODO: Insert tweeds into threads
                    // foreach (var item in batch.Items) await ProcessTweed(item.Result, session);
                    await session.SaveChangesAsync(stoppingToken);
                }, stoppingToken);

                return;
            }
            catch (Exception e)
            {
                _logger.LogError("Failure in subscription: " + subscriptionName, e);

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
                await subscriptionWorker.DisposeAsync();
            }
        }
    }

    private async Task ProcessTweed(Model.Tweed tweed, IAsyncDocumentSession session)
    {
        // find Thread with parent Tweed
        // Insert Tweed into Thread

        ThreadQueries threadQueries = new(session);
        await threadQueries.AddTweedToThread(tweed.Id!, tweed.ParentTweedId!, tweed.ThreadId!);
    }
}