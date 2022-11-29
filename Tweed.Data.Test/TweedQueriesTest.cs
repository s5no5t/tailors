using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NodaTime;
using Raven.Client.Documents.Session;
using Xunit;

namespace Tweed.Data.Test;

public class TweedQueriesTest
{
    private static readonly ZonedDateTime FixedZonedDateTime =
        new(new LocalDateTime(2022, 11, 18, 15, 20), DateTimeZone.Utc, new Offset());

    [Fact]
    public async Task StoreTweed_SavesTweed()
    {
        var session = new Mock<IAsyncDocumentSession>();

        var queries = new TweedQueries(session.Object);
        var tweed = new Entities.Tweed
        {
            CreatedAt = FixedZonedDateTime,
            AuthorId = "123"
        };
        await queries.StoreTweed(tweed);

        session.Verify(s => s.StoreAsync(tweed, default));
    }

    [Fact]
    public async Task StoreTweed_ValidatesAuthorId()
    {
        var session = new Mock<IAsyncDocumentSession>();

        var queries = new TweedQueries(session.Object);
        var tweed = new Entities.Tweed
        {
            CreatedAt = FixedZonedDateTime
        };
        
        await Assert.ThrowsAsync<ArgumentException>(async () => await queries.StoreTweed(tweed));
    }

    [Fact]
    public async Task StoreTweed_ValidatesCreatedAt()
    {
        var session = new Mock<IAsyncDocumentSession>();

        var queries = new TweedQueries(session.Object);
        var tweed = new Entities.Tweed
        {
            AuthorId = "123"
        };
        
        await Assert.ThrowsAsync<ArgumentException>(async () => await queries.StoreTweed(tweed));
    }

    [Fact]
    public async Task GetLatestTweeds_ShouldReturnTweeds()
    {
        using var ravenDb = new RavenTestDb();
        using var session = ravenDb.OpenAsyncSession();

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
        using var ravenDb = new RavenTestDb();
        using var session = ravenDb.OpenAsyncSession();

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
        using var ravenDb = new RavenTestDb();
        using var session = ravenDb.OpenAsyncSession();

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
        using var ravenDb = new RavenTestDb();
        using var session = ravenDb.OpenAsyncSession();

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
        using var ravenDb = new RavenTestDb();
        using var session = ravenDb.OpenAsyncSession();

        var queries = new TweedQueries(session);
        var tweed = await queries.GetById("invalid");
        
        Assert.Null(tweed);
    }
}
