using System.Linq;
using System.Threading.Tasks;
using Moq;
using NodaTime;
using Raven.Client.Documents.Session;
using Xunit;

namespace Tweed.Data.Test;

public class TweedQueriesTest
{
    [Fact]
    public async Task CreateTweed_UpdatesCreatedAt()
    {
        using var ravenDb = new RavenTestDb();
        using var session = ravenDb.Session;

        var queries = new TweedQueries(session);
        var tweed = new Models.Tweed();
        await queries.CreateTweed(tweed);

        Assert.NotNull(tweed.CreatedAt);
    }

    [Fact]
    public async Task CreateTweed_SavesTweed()
    {
        var session = new Mock<IAsyncDocumentSession>();

        var queries = new TweedQueries(session.Object);
        var tweed = new Models.Tweed();
        await queries.CreateTweed(tweed);

        session.Verify(s => s.StoreAsync(tweed, default));
    }

    [Fact]
    public async Task GetLatestTweeds_ShouldReturnTweeds()
    {
        using var ravenDb = new RavenTestDb();
        using var session = ravenDb.Session;

        Models.Tweed tweed = new()
        {
            Content = "test"
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
        using var session = ravenDb.Session;

        var older = new ZonedDateTime(new LocalDateTime(2022, 11, 18, 15, 20), DateTimeZone.Utc, new Offset());
        Models.Tweed olderTweed = new()
        {
            Content = "older tweed",
            CreatedAt = older
        };
        await session.StoreAsync(olderTweed);
        var recent = older.PlusHours(1);
        Models.Tweed recentTweed = new()
        {
            Content = "recent tweed",
            CreatedAt = recent
        };
        await session.StoreAsync(recentTweed);
        await session.SaveChangesAsync();

        var queries = new TweedQueries(session);
        var tweeds = (await queries.GetLatestTweeds()).ToList();
        Assert.Equal(tweeds[0], recentTweed);
        Assert.Equal(tweeds[1], olderTweed);
    }
}
