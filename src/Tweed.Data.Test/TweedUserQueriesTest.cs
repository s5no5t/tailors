using System.Collections.Generic;
using System.Threading.Tasks;
using NodaTime;
using Raven.Client.Documents;
using Tweed.Data.Entities;
using Xunit;

namespace Tweed.Data.Test;

[Collection("RavenDb Collection")]
public class TweedUserQueriesTest
{
    private static readonly ZonedDateTime FixedZonedDateTime =
        new(new LocalDateTime(2022, 11, 18, 15, 20), DateTimeZone.Utc, new Offset());
    
    private readonly IDocumentStore _store;

    public TweedUserQueriesTest(RavenTestDbFixture ravenDb)
    {
        _store = ravenDb.CreateDocumentStore();
    }
    
    [Fact]
    public async Task AddFollower_ShouldAddFollower()
    {
        using var session = _store.OpenAsyncSession();
        TweedUser user = new()
        {
            Id = "userId"
        };
        await session.StoreAsync(user);
        await session.SaveChangesAsync();
        TweedUserQueries queries = new(session);

        await queries.AddFollower("leaderId", "userId", FixedZonedDateTime);

        Assert.Equal("leaderId", user.Follows[0].LeaderId);
    }

    [Fact]
    public async Task AddFollower_ShouldNotAddFollower_WhenAlreadyFollowed()
    {
        using var session = _store.OpenAsyncSession();
        TweedIdentityUser user = new()
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
        await session.SaveChangesAsync();
        TweedUserQueries queries = new(session);

        await queries.AddFollower("leaderId", "userId", FixedZonedDateTime);

        Assert.Single(user.Follows);
    }

    [Fact]
    public async Task RemoveFollower_ShouldRemoveFollower()
    {
        using var session = _store.OpenAsyncSession();
        TweedIdentityUser user = new()
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

        TweedUserQueries queries = new(session);
        await queries.RemoveFollower("leaderId", "userId");

        var userAfterQuery = await session.LoadAsync<TweedIdentityUser>("userId");
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

        TweedIdentityUser leader = new()
        {
            Id = "leaderId"
        };
        await session.StoreAsync(leader);
        for (var i = 0; i < givenFollowerCount; i++)
        {
            TweedIdentityUser follower = new()
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
        TweedUserQueries queries = new(session);

        var followerCount = await queries.GetFollowerCount("leaderId");

        Assert.Equal(givenFollowerCount, followerCount);
    }
    
    [Fact]
    public async Task AddLike_ShouldIncreaseLikes()
    {
        using var session = _store.OpenAsyncSession();
        var appUser = new TweedIdentityUser
        {
            Id = "currentUser"
        };
        await session.StoreAsync(appUser);
        var tweed = new Entities.Tweed
        {
            Id = "tweedId"
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var queries = new TweedUserQueries(session);

        await queries.AddLike("tweedId", "currentUser", FixedZonedDateTime);

        Assert.Single(appUser.Likes);
    }

    [Fact]
    public async Task AddLike_ShouldIncreaseLikesCounter()
    {
        using var session = _store.OpenAsyncSession();
        var appUser = new TweedIdentityUser
        {
            Id = "currentUser"
        };
        await session.StoreAsync(appUser);
        var tweed = new Entities.Tweed
        {
            Id = "tweedId"
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var queries = new TweedUserQueries(session);

        await queries.AddLike("tweedId", "currentUser", FixedZonedDateTime);
        await session.SaveChangesAsync();

        var likesCounter = await session.CountersFor(tweed.Id).GetAsync("Likes");
        Assert.Equal(1, likesCounter);
    }

    [Fact]
    public async Task AddLike_ShouldNotIncreaseLikes_WhenUserHasAlreadyLiked()
    {
        using var session = _store.OpenAsyncSession();
        var appUser = new TweedIdentityUser
        {
            Id = "currentUser",
            Likes = new List<TweedLike>
            {
                new()
                {
                    TweedId = "tweedId"
                }
            }
        };
        await session.StoreAsync(appUser);
        await session.SaveChangesAsync();
        var queries = new TweedUserQueries(session);

        await queries.AddLike("tweedId", "currentUser", FixedZonedDateTime);

        Assert.Single(appUser.Likes);
    }

    [Fact]
    public async Task RemoveLike_ShouldDecreaseLikes()
    {
        using var session = _store.OpenAsyncSession();
        var appUser = new TweedIdentityUser
        {
            Id = "currentUser",
            Likes = new List<TweedLike>
            {
                new()
                {
                    TweedId = "tweedId"
                }
            }
        };
        await session.StoreAsync(appUser);
        var tweed = new Entities.Tweed
        {
            Id = "tweedId"
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var queries = new TweedUserQueries(session);

        await queries.RemoveLike("tweedId", "currentUser");

        Assert.Empty(appUser.Likes);
    }

    [Fact]
    public async Task RemoveLike_ShouldDecreaseLikesCounter()
    {
        using var session = _store.OpenAsyncSession();
        var appUser = new TweedIdentityUser
        {
            Id = "userId"
        };
        await session.StoreAsync(appUser);
        var tweed = new Entities.Tweed
        {
            Id = "tweedId"
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        session.CountersFor(tweed.Id).Increment("Likes");
        await session.SaveChangesAsync();
        var queries = new TweedUserQueries(session);

        await queries.RemoveLike(tweed.Id, "userId");
        await session.SaveChangesAsync();

        var likesCounter = await session.CountersFor(tweed.Id).GetAsync("Likes");
        Assert.Equal(0, likesCounter);
    }

    [Fact]
    public async Task RemoveLike_ShouldNotDecreaseLikes_WhenUserAlreadyDoesntLike()
    {
        using var session = _store.OpenAsyncSession();
        var appUser = new TweedIdentityUser
        {
            Id = "userId"
        };
        await session.StoreAsync(appUser);
        var tweed = new Entities.Tweed
        {
            Id = "tweedId"
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var queries = new TweedUserQueries(session);

        await queries.RemoveLike("tweedId", "userId");

        Assert.Empty(appUser.Likes);
    }
}
