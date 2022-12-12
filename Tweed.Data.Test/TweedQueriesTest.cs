using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NodaTime;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Tweed.Data.Entities;
using Xunit;

namespace Tweed.Data.Test;

[Collection("RavenDb Collection")]
public class TweedQueriesTest : IClassFixture<RavenTestDbFixture>
{
    private static readonly ZonedDateTime FixedZonedDateTime =
        new(new LocalDateTime(2022, 11, 18, 15, 20), DateTimeZone.Utc, new Offset());

    private readonly IDocumentStore _store;

    public TweedQueriesTest(RavenTestDbFixture ravenDb)
    {
        _store = ravenDb.CreateDocumentStore();
    }

    [Fact]
    public async Task StoreTweed_SavesTweed()
    {
        var session = new Mock<IAsyncDocumentSession>();
        var queries = new TweedQueries(session.Object);
        await queries.StoreTweed("text", "user1", FixedZonedDateTime);

        session.Verify(s => s.StoreAsync(It.IsAny<Entities.Tweed>(), default));
    }

    [Fact]
    public async Task GetFeed_ShouldReturnTweedsByCurrentUser()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        var currentUser = new AppUser
        {
            Id = "currentUser"
        };
        await session.StoreAsync(currentUser);
        Entities.Tweed currentUserTweed = new()
        {
            Text = "test",
            AuthorId = "currentUser"
        };
        await session.StoreAsync(currentUserTweed);
        Entities.Tweed otherUserTweed = new()
        {
            Text = "test",
            AuthorId = "otherUser"
        };
        await session.StoreAsync(otherUserTweed);
        await session.SaveChangesAsync();
        var queries = new TweedQueries(session);

        var tweeds = await queries.GetFeed("currentUser");

        Assert.Contains(currentUserTweed.Id, tweeds.Select(t => t.Id));
        Assert.DoesNotContain(otherUserTweed.Id, tweeds.Select(t => t.Id));
    }

    [Fact]
    public async Task GetFeed_ShouldReturnTweedsByFollowedUsers()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        var currentUser = new AppUser
        {
            Id = "currentUser",
            Follows = new List<Follows>
            {
                new()
                {
                    LeaderId = "followedUser"
                }
            }
        };
        await session.StoreAsync(currentUser);
        var followedUser = new AppUser
        {
            Id = "followedUser"
        };
        await session.StoreAsync(followedUser);
        Entities.Tweed followedUserTweed = new()
        {
            Text = "test",
            AuthorId = "followedUser"
        };
        await session.StoreAsync(followedUserTweed);
        Entities.Tweed notFollowedUserTweed = new()
        {
            Text = "test",
            AuthorId = "notFollowedUser"
        };
        await session.StoreAsync(notFollowedUserTweed);
        await session.SaveChangesAsync();
        var queries = new TweedQueries(session);

        var tweeds = await queries.GetFeed("currentUser");

        Assert.Contains(tweeds, t => t.Id == followedUserTweed.Id);
        Assert.DoesNotContain(tweeds, t => t.Id == notFollowedUserTweed.Id);
    }

    [Fact]
    public async Task GetFeed_ShouldReturnOrderedTweeds()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        var currentUser = new AppUser
        {
            Id = "currentUser"
        };
        await session.StoreAsync(currentUser);
        Entities.Tweed olderTweed = new()
        {
            Text = "older tweed",
            AuthorId = "currentUser",
            CreatedAt = FixedZonedDateTime
        };
        await session.StoreAsync(olderTweed);
        var recent = FixedZonedDateTime.PlusHours(1);
        Entities.Tweed recentTweed = new()
        {
            Text = "recent tweed",
            AuthorId = "currentUser",
            CreatedAt = recent
        };
        await session.StoreAsync(recentTweed);
        await session.SaveChangesAsync();
        var queries = new TweedQueries(session);

        var tweeds = (await queries.GetFeed("currentUser")).ToList();

        Assert.Equal(recentTweed.Id, tweeds[0].Id);
        Assert.Equal(olderTweed.Id, tweeds[1].Id);
    }

    [Fact]
    public async Task GetFeed_ShouldReturn20Tweeds()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        var currentUser = new AppUser
        {
            Id = "currentUser"
        };
        await session.StoreAsync(currentUser);
        for (var i = 0; i < 25; i++)
        {
            Entities.Tweed tweed = new()
            {
                Text = "test",
                AuthorId = "currentUser",
                CreatedAt = FixedZonedDateTime
            };
            await session.StoreAsync(tweed);
        }

        await session.SaveChangesAsync();
        var queries = new TweedQueries(session);

        var tweeds = (await queries.GetFeed("currentUser")).ToList();

        Assert.Equal(20, tweeds.Count);
    }

    [Fact]
    public async Task GetById_ShouldReturnTweed()
    {
        using var session = _store.OpenAsyncSession();
        Entities.Tweed tweed = new()
        {
            Text = "test",
            CreatedAt = FixedZonedDateTime
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var queries = new TweedQueries(session);

        var tweed2 = await queries.GetById(tweed.Id);

        Assert.Equal(tweed.Id, tweed2?.Id);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnNull()
    {
        using var session = _store.OpenAsyncSession();
        var queries = new TweedQueries(session);

        var tweed = await queries.GetById("invalid");

        Assert.Null(tweed);
    }

    [Fact]
    public async Task GetTweedsForUser_ShouldReturnTweeds()
    {
        using var session = _store.OpenAsyncSession();
        Entities.Tweed tweed = new()
        {
            Text = "test",
            AuthorId = "user"
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var queries = new TweedQueries(session);

        var tweeds = await queries.GetTweedsForUser("user");

        Assert.NotEmpty(tweeds);
    }

    [Fact]
    public async Task GetTweedsForUser_ShouldReturnOrderedTweeds()
    {
        using var session = _store.OpenAsyncSession();
        Entities.Tweed olderTweed = new()
        {
            Text = "older tweed",
            CreatedAt = FixedZonedDateTime,
            AuthorId = "user1"
        };
        await session.StoreAsync(olderTweed);
        var recent = FixedZonedDateTime.PlusHours(1);
        Entities.Tweed recentTweed = new()
        {
            Text = "recent tweed",
            CreatedAt = recent,
            AuthorId = "user1"
        };
        await session.StoreAsync(recentTweed);
        await session.SaveChangesAsync();
        var queries = new TweedQueries(session);

        var tweeds = (await queries.GetTweedsForUser("user1")).ToList();

        Assert.Equal(recentTweed, tweeds[0]);
        Assert.Equal(olderTweed, tweeds[1]);
    }

    [Fact]
    public async Task GetTweedsForUser_ShouldReturn20Tweeds()
    {
        using var session = _store.OpenAsyncSession();
        for (var i = 0; i < 25; i++)
        {
            Entities.Tweed tweed = new()
            {
                Text = "test",
                CreatedAt = FixedZonedDateTime,
                AuthorId = "user1"
            };
            await session.StoreAsync(tweed);
        }

        await session.SaveChangesAsync();
        var queries = new TweedQueries(session);

        var tweeds = (await queries.GetTweedsForUser("user1")).ToList();

        Assert.Equal(20, tweeds.Count);
    }

    [Fact]
    public async Task GetTweedsForUser_ShouldNotReturnTweedsFromOtherUsers()
    {
        using var session = _store.OpenAsyncSession();
        Entities.Tweed tweed = new()
        {
            Text = "test",
            CreatedAt = FixedZonedDateTime,
            AuthorId = "user2"
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var queries = new TweedQueries(session);

        var tweeds = (await queries.GetTweedsForUser("user1")).ToList();

        Assert.Empty(tweeds);
    }

    [Fact]
    public async Task AddLike_ShouldIncreaseLikes()
    {
        using var session = _store.OpenAsyncSession();
        Entities.Tweed tweed = new()
        {
            Text = "test",
            CreatedAt = FixedZonedDateTime
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var queries = new TweedQueries(session);

        await queries.AddLike(tweed.Id, "currentUser", FixedZonedDateTime);

        Assert.Single(tweed.Likes);
    }

    [Fact]
    public async Task AddLike_ShouldNotIncreaseLikes_WhenUserHasAlreadyLiked()
    {
        using var session = _store.OpenAsyncSession();
        Entities.Tweed tweed = new()
        {
            Text = "test",
            CreatedAt = FixedZonedDateTime,
            Likes = new List<Like>
            {
                new()
                {
                    UserId = "currentUser"
                }
            }
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var queries = new TweedQueries(session);

        await queries.AddLike(tweed.Id, "currentUser", FixedZonedDateTime);

        Assert.Single(tweed.Likes);
    }

    [Fact]
    public async Task RemoveLike_ShouldDecreaseLikes()
    {
        using var session = _store.OpenAsyncSession();
        Entities.Tweed tweed = new()
        {
            Text = "test",
            CreatedAt = FixedZonedDateTime,
            Likes = new List<Like> { new() { UserId = "currentUser" } }
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var queries = new TweedQueries(session);

        await queries.RemoveLike(tweed.Id, "currentUser");

        Assert.Empty(tweed.Likes);
    }

    [Fact]
    public async Task RemoveLike_ShouldNotDecreaseLikes_WhenUserAlreadyDoesntLike()
    {
        using var session = _store.OpenAsyncSession();
        Entities.Tweed tweed = new()
        {
            Text = "test",
            CreatedAt = FixedZonedDateTime,
            Likes = new List<Like>()
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var queries = new TweedQueries(session);

        await queries.RemoveLike(tweed.Id, "currentUser");

        Assert.Empty(tweed.Likes);
    }

    [Fact]
    public async Task GetLikesCount_ShouldReturn1_WhenTweedHasLike()
    {
        using var session = _store.OpenAsyncSession();
        Entities.Tweed tweed = new()
        {
            Text = "test",
            CreatedAt = FixedZonedDateTime,
            Likes = new List<Like>
            {
                new()
                {
                    UserId = "userId"
                }
            }
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var queries = new TweedQueries(session);

        var likesCount = await queries.GetLikesCount(tweed.Id);

        Assert.Equal(1, likesCount);
    }
}
