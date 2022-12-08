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
    private readonly IDocumentStore _store;

    public AppUserQueriesTest(RavenTestDbFixture ravenDb)
    {
        _store = ravenDb.CreateDocumentStore();
    }

    [Fact]
    public async Task RemoveFollower()
    {
        using var session = _store.OpenAsyncSession();
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
    public async Task GetFollowerCount_ShouldReturn0_WithoutFollowers()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();

        AppUser leader = new()
        {
            Id = "userId"
        };
        await session.StoreAsync(leader);
        await session.SaveChangesAsync();
        AppUserQueries queries = new(session);
        
        var followerCount = await queries.GetFollowerCount("userId");

        Assert.Equal(0, followerCount);
    }
    
    [Fact]
    public async Task GetFollowerCount_ShouldReturn1_WhenThereIsOneFollower()
    {
        using var session = _store.OpenAsyncSession();
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
        AppUserQueries queries = new(session);
        
        var followerCount = await queries.GetFollowerCount("leaderId");

        Assert.Equal(1, followerCount);
    }
}
