using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NodaTime;
using Raven.Client.Documents.Session;
using Tweed.Data.Entities;
using Xunit;

namespace Tweed.Data.Test;

public class TweedQueriesTest : IClassFixture<RavenTestDbFixture>
{
    private static readonly ZonedDateTime FixedZonedDateTime =
        new(new LocalDateTime(2022, 11, 18, 15, 20), DateTimeZone.Utc, new Offset());

    private readonly RavenTestDbFixture _ravenDb;

    public TweedQueriesTest(RavenTestDbFixture ravenDb)
    {
        _ravenDb = ravenDb;
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
    public async Task GetLatestTweeds_ShouldReturnTweeds()
    {
        using var store = _ravenDb.CreateDocumentStore();
        using var session = store.OpenAsyncSession();

        Entities.Tweed tweed = new()
        {
            Text = "test"
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();

        var queries = new TweedQueries(session);
        var tweeds = await queries.GetLatestTweeds();

        Assert.NotEmpty(tweeds);
    }

    [Fact]
    public async Task GetLatestTweeds_ShouldReturnOrderedTweeds()
    {
        using var store = _ravenDb.CreateDocumentStore();
        using var session = store.OpenAsyncSession();

        Entities.Tweed olderTweed = new()
        {
            Text = "older tweed",
            CreatedAt = FixedZonedDateTime
        };
        await session.StoreAsync(olderTweed);
        var recent = FixedZonedDateTime.PlusHours(1);
        Entities.Tweed recentTweed = new()
        {
            Text = "recent tweed",
            CreatedAt = recent
        };
        await session.StoreAsync(recentTweed);
        await session.SaveChangesAsync();

        var queries = new TweedQueries(session);
        var tweeds = (await queries.GetLatestTweeds()).ToList();

        Assert.Equal(recentTweed, tweeds[0]);
        Assert.Equal(olderTweed, tweeds[1]);
    }

    [Fact]
    public async Task GetLatestTweeds_ShouldReturn20Tweeds()
    {
        using var store = _ravenDb.CreateDocumentStore();
        using var session = store.OpenAsyncSession();

        for (var i = 0; i < 25; i++)
        {
            Entities.Tweed tweed = new()
            {
                Text = "test",
                CreatedAt = FixedZonedDateTime
            };
            await session.StoreAsync(tweed);
        }

        await session.SaveChangesAsync();

        var queries = new TweedQueries(session);
        var tweeds = (await queries.GetLatestTweeds()).ToList();

        Assert.Equal(20, tweeds.Count);
    }

    [Fact]
    public async Task GetById_ShouldReturnTweed()
    {
        using var store = _ravenDb.CreateDocumentStore();
        using var session = store.OpenAsyncSession();

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
        using var store = _ravenDb.CreateDocumentStore();
        using var session = store.OpenAsyncSession();

        var queries = new TweedQueries(session);
        var tweed = await queries.GetById("invalid");

        Assert.Null(tweed);
    }

    [Fact]
    public async Task AddLike_ShouldIncreaseLikes()
    {
        using var store = _ravenDb.CreateDocumentStore();
        using var session = store.OpenAsyncSession();

        Entities.Tweed tweed = new()
        {
            Text = "test",
            CreatedAt = FixedZonedDateTime
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();

        var queries = new TweedQueries(session);
        await queries.AddLike(tweed.Id, "user1", FixedZonedDateTime);

        Assert.Equal(1, tweed.LikedBy.Count);
    }

    [Fact]
    public async Task AddLike_ShouldNotIncreaseLikes_WhenUserHasAlreadyLiked()
    {
        using var store = _ravenDb.CreateDocumentStore();
        using var session = store.OpenAsyncSession();

        Entities.Tweed tweed = new()
        {
            Text = "test",
            CreatedAt = FixedZonedDateTime,
            LikedBy = new List<LikedBy>
            {
                new()
                {
                    UserId = "user1"
                }
            }
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();

        var queries = new TweedQueries(session);
        await queries.AddLike(tweed.Id, "user1", FixedZonedDateTime);

        Assert.Equal(1, tweed.LikedBy.Count);
    }
}
