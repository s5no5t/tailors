using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using Raven.Client.Exceptions;
using Raven.Client.Exceptions.Database;
using Raven.Client.NodaTime;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using Tweed.Data.Entities;
using Tweed.Data.Indexes;

namespace Tweed.Data;

public static class RavenExtensions
{
    public static IDocumentStore EnsureDatabaseExists(this IDocumentStore store)
    {
        try
        {
            store.Maintenance.ForDatabase(store.Database).Send(new GetStatisticsOperation());
        }
        catch (DatabaseDoesNotExistException)
        {
            try
            {
                store.Maintenance.Server.Send(
                    new CreateDatabaseOperation(new DatabaseRecord(store.Database)));
            }
            catch (ConcurrencyException)
            {
                // The database was already created before calling CreateDatabaseOperation
            }
        }

        return store;
    }

    public static void PreInitialize(this IDocumentStore store)
    {
        store.ApplyCustomConventions();
        store.ConfigureForNodaTime();
    }

    private static void ApplyCustomConventions(this IDocumentStore store)
    {
        store.Conventions.RegisterAsyncIdConvention<AppUserFollows>((s, follows) =>
            Task.FromResult(AppUserFollows.BuildId(follows.AppUserId)));
    }

    public static void DeployIndexes(this IDocumentStore store)
    {
        new Tweeds_ByAuthorIdAndCreatedAt().Execute(store);
        new Tweeds_ByText().Execute(store);
        new AppUserFollows_FollowerCount().Execute(store);
        new AppUsers_ByUserName().Execute(store);
    }
}
