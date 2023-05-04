using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Raven.Client.Documents.Subscriptions;
using Raven.Client.Exceptions.Database;
using Raven.Client.Exceptions.Documents.Subscriptions;
using Raven.Client.Exceptions.Security;
using Tweed.Data.Domain;

namespace Tweed.Web;

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
        await EnsureSubScriptionExists(stoppingToken);

        while (true)
        {
            var subscriptionWorker =
                _store.Subscriptions.GetSubscriptionWorker<Data.Model.Tweed>(SubscriptionName);

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
                    using var session = batch.OpenAsyncSession();
                    // TODO: Insert tweeds into threads
                    // foreach (var item in batch.Items) await ProcessTweed(item.Result, session);
                    await session.SaveChangesAsync(stoppingToken);
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
            SubscriptionCreationOptions options = new()
            {
                Name = SubscriptionName
            };
            await _store.Subscriptions.CreateAsync<Data.Model.Tweed>(
                t => !ReferenceEquals(t.ParentTweedId, null), options, token: stoppingToken);
        }
    }

    private async Task ProcessTweed(Data.Model.Tweed tweed, IAsyncDocumentSession session)
    {
        ThreadQueries threadQueries = new(session);

        var thread = await threadQueries.FindOrCreateThreadForTweed(tweed.Id!);
        await threadQueries.AddReplyToThread(thread.Id!, tweed.Id!, tweed.ParentTweedId!);
    }
}
