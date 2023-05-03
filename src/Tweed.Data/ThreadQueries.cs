using Raven.Client.Documents.Session;

namespace Tweed.Data;

public class ThreadQueries
{
    private readonly IAsyncDocumentSession _session;

    public ThreadQueries(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task UpdateThread(string tweedId, string parentTweedId, string rootTweedId)
    {
        throw new NotImplementedException();
    }
}
