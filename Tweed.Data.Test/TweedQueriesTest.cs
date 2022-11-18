using System.Threading.Tasks;
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
}
