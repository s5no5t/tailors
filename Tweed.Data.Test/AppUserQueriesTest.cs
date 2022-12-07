using System.Collections.Generic;
using System.Threading.Tasks;
using Tweed.Data.Entities;
using Xunit;

namespace Tweed.Data.Test;

public class AppUserQueriesTest : IClassFixture<RavenTestDbFixture>
{
    private readonly RavenTestDbFixture _ravenDb;

    public AppUserQueriesTest(RavenTestDbFixture ravenDb)
    {
        _ravenDb = ravenDb;
    }

    [Fact]
    public async Task RemoveFollower()
    {
        using var store = _ravenDb.CreateDocumentStore();
        using var session = store.OpenAsyncSession();

        AppUser user = new()
        {
            Id = "userId",
            Follows = new List<Follows>
            {
                new()
                {
                    LeaderId = "leaderId"
                }
            }
        };
        await session.StoreAsync(user);
        AppUserQueries queries = new(session);

        await queries.RemoveFollower("leaderId", "userId");

        var userAfterQuery = await session.LoadAsync<AppUser>("userId");
        Assert.DoesNotContain(userAfterQuery.Follows, u => u.LeaderId == "leaderId");
    }
}
