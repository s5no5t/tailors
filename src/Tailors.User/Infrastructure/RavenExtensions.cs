using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using Raven.Client.Exceptions;
using Raven.Client.Exceptions.Database;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using Tailors.User.Domain;
using Tailors.User.Domain.UserFollowsAggregate;
using Tailors.User.Infrastructure.Indexes;

namespace Tailors.User.Infrastructure;

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
    }

    private static void ApplyCustomConventions(this IDocumentStore store)
    {
        store.Conventions.RegisterAsyncIdConvention<UserFollows>((_, follows) =>
        {
            ArgumentNullException.ThrowIfNull(follows.UserId);
            return Task.FromResult(UserFollows.BuildId(follows.UserId));
        });
    }

    public static void DeployUserIndexes(this IDocumentStore store)
    {
        new UserFollows_FollowerCount().Execute(store);
        new Users_ByUserName().Execute(store);
    }
}
