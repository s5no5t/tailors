using System.Threading.Tasks;
using Xunit;

namespace Tweed.Data.Test;

public class TweedQueriesTest
{
    [Fact(Skip = "Stub")]
    public async Task SaveTweed()
    {
        using var ravenDb = new RavenTestDb();
        using var session = ravenDb.Session;

        var queries = new TweedQueries();
        await queries.SaveTweed(new Models.Tweed());
    }
}
