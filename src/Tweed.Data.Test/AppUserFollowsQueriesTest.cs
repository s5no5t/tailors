using System.Collections.Generic;
using System.Threading.Tasks;
using NodaTime;
using Raven.Client.Documents;
using Tweed.Data.Entities;
using Xunit;

namespace Tweed.Data.Test;

[Collection("RavenDb Collection")]
public class AppUserFollowsQueriesTest
{
    private readonly IDocumentStore _store;
    private static readonly ZonedDateTime FixedZonedDateTime =
        new(new LocalDateTime(2022, 11, 18, 15, 20), DateTimeZone.Utc, new Offset());

    public AppUserFollowsQueriesTest(RavenTestDbFixture ravenDb)
    {
        _store = ravenDb.CreateDocumentStore();
    }

    [Fact]
    public async Task AddFollower_ShouldAddFollower()
    {
        using var session = _store.OpenAsyncSession();
        AppUser user = new()
        {
            Id = "userId"
        };
        await session.StoreAsync(user);
        AppUserFollows appUserFollows = new()
        {
            AppUserId = user.Id
        };
        await session.StoreAsync(appUserFollows);
        await session.SaveChangesAsync();
        AppUserFollowsQueries queries = new(session);

        await queries.AddFollower("leaderId", "userId", FixedZonedDateTime);

        Assert.Equal("leaderId", appUserFollows.Follows[0].LeaderId);
    }

    [Fact]
    public async Task AddFollower_ShouldNotAddFollower_WhenAlreadyFollowed()
    {
        using var session = _store.OpenAsyncSession();
        AppUser user = new()
        {
            Id = "userId",
            
        };
        await session.StoreAsync(user);
        AppUserFollows appUserFollows = new()
        {
            AppUserId = user.Id,
            Follows = new List<Follows>
            {
                new()
                {
                    LeaderId = "leaderId"
                }
            }
        };
        await session.StoreAsync(appUserFollows);
        await session.SaveChangesAsync();
        AppUserFollowsQueries queries = new(session);

        await queries.AddFollower("leaderId", "userId", FixedZonedDateTime);

        Assert.Single(appUserFollows.Follows);
    }

    [Fact]
    public async Task RemoveFollower_ShouldRemoveFollower()
    {
        using var session = _store.OpenAsyncSession();
        AppUser user = new()
        {
            Id = "userId",
        };
        await session.StoreAsync(user);
        AppUserFollows appUserFollows = new()
        {
            AppUserId = user.Id,
            Follows = new List<Follows>
            {
                new()
                {
                    LeaderId = "leaderId"
                }
            }
        };
        await session.StoreAsync(appUserFollows);

        AppUserFollowsQueries queries = new(session);
        await queries.RemoveFollower("leaderId", "userId");

        var userAfterQuery = await session.LoadAsync<AppUserFollows>(AppUserFollows.BuildId("userId"));
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
                
            };
            await session.StoreAsync(follower);
            AppUserFollows appUserFollows = new()
            {
                AppUserId = follower.Id,
                Follows = new List<Follows>
                {
                    new()
                    {
                        LeaderId = "leaderId"
                    }
                }
            };
            await session.StoreAsync(appUserFollows);
        }

        await session.SaveChangesAsync();
        AppUserFollowsQueries queries = new(session);

        var followerCount = await queries.GetFollowerCount("leaderId");

        Assert.Equal(givenFollowerCount, followerCount);
    }
}
