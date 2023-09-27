using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using Raven.Client.Exceptions;
using Raven.Client.Exceptions.Database;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using Tailors.Infrastructure.Tweed.Indexes;
using Tailors.Infrastructure.UserFollows.Indexes;

namespace Tailors.Infrastructure;

public static class RavenDbSetup
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
    }
    
    private static void ApplyCustomConventions(this IDocumentStore store)
    {
        store.Conventions.RegisterAsyncIdConvention<Domain.UserLikes.UserLikes>((_, userLikes) =>
        {
            ArgumentNullException.ThrowIfNull(userLikes.UserId);
            return Task.FromResult(Domain.UserLikes.UserLikes.BuildId(userLikes.UserId));
        });
        store.Conventions.RegisterAsyncIdConvention<Domain.UserFollows.UserFollows>((_, follows) =>
        {
            ArgumentNullException.ThrowIfNull(follows.UserId);
            return Task.FromResult(Domain.UserFollows.UserFollows.BuildId(follows.UserId));
        });
    }
    
    public static void DeployIndexes(this IDocumentStore store)
    {
        new UserFollowsFollowerCount().Execute(store);
        new UsersByUserName().Execute(store);
        new TweedsByAuthorIdAndCreatedAt().Execute(store);
        new TweedsByText().Execute(store);
    }
}
