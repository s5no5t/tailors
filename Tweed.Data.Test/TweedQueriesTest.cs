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
}
