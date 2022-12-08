using System.Collections.Generic;
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
    public async Task RemoveFollower_ShouldRemoveFollower()
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

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    public async Task GetFollowerCount_ShouldReturnFollowerCount(int givenFollowerCount)
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();

        AppUser leader = new()
        {
            Id = "leaderId"
        };
        await session.StoreAsync(leader);
        for (var i = 0; i < givenFollowerCount; i++)
        {
            AppUser follower = new()
            {
                Id = $"follower/${i}",
                Follows = new List<Follows>
                {
                    new()
                    {
                        LeaderId = "leaderId"
                    }
                }
            };
            await session.StoreAsync(follower);
        }
        await session.SaveChangesAsync();
        AppUserQueries queries = new(session);

        var followerCount = await queries.GetFollowerCount("leaderId");

        Assert.Equal(givenFollowerCount, followerCount);
    }
}
