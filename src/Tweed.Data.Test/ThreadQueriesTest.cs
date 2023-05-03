using System.Threading.Tasks;
using Raven.Client.Documents;
using Tweed.Data.Model;
using Xunit;

namespace Tweed.Data.Test;

[Collection("RavenDb Collection")]
public class ThreadQueriesTest
{
    private readonly IDocumentStore _store;
    
    public ThreadQueriesTest(RavenTestDbFixture ravenDb)
    {
        _store = ravenDb.CreateDocumentStore();
    }

    [Fact]
    public async Task AddTweedToThread_ShouldCreateThread_IfItDoesntExist()
    {
        using var session = _store.OpenAsyncSession();

        ThreadQueries queries = new(session);

        await queries.AddTweedToThread("tweedId", "parentTweedId", "threadId");

        var thread = session.LoadAsync<Thread>("threadId");
        Assert.NotNull(thread);
    } 
}