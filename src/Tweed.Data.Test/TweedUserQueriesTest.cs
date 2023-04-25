using Raven.Client.Documents;
using Xunit;

namespace Tweed.Data.Test;

[Collection("RavenDb Collection")]
public class TweedUserQueriesTest
{
    private readonly IDocumentStore _store;

    public TweedUserQueriesTest(RavenTestDbFixture ravenDb)
    {
        _store = ravenDb.CreateDocumentStore();
    }
}
