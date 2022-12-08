using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Tweed.Data.Entities;
using Xunit;

namespace Tweed.Data.Test;

[Collection("RavenDb Collection")]
public class AppUserQueriesTest
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

    [Fact]
    public async Task GetFollowerCount_ShouldReturnFollowerCount()
    {
        using var store = _ravenDb.CreateDocumentStore();
        using var session = store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();

        AppUser leader = new()
        {
            Id = "leaderId"
        };
        await session.StoreAsync(leader);
        AppUser follower = new()
        {
            Id = "followerId",
            Follows = new List<Follows>
            {
                new()
                {
                    LeaderId = "leaderId"
                }
            }
        };
        await session.StoreAsync(follower);
        await session.SaveChangesAsync();

        var result = await session
            .Query<AppUsers_FollowerCount.Result, AppUsers_FollowerCount>()
            .Where(u => u.AppUserId == "leaderId")
            .FirstOrDefaultAsync();

        Assert.NotNull(result);
        Assert.Equal(1, result.FollowerCount);
    }
}
