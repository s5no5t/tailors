using System.Linq;
using System.Threading.Tasks;
using NodaTime;
using Xunit;

namespace Tweed.Data.Test;

public class TweedQueriesTest
{
    [Fact]
    public async Task SaveTweed_SavesTweed()
    {
        using var ravenDb = new RavenTestDb();
        using var session = ravenDb.Session;

        var queries = new TweedQueries(session);
        await queries.SaveTweed(new Models.Tweed());
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

        var old = new LocalDateTime(2022, 11, 18, 15, 20);
        Models.Tweed oldTweed = new()
        {
            Content = "old tweed",
            CreatedAt = old
        };
        await session.StoreAsync(oldTweed);
        var recent = old.PlusDays(1);
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
        Assert.Equal(tweeds[1], oldTweed);
    }
}
