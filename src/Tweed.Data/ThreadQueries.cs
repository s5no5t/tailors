using Raven.Client.Documents.Session;
using Tweed.Data.Model;

namespace Tweed.Data;

public class ThreadQueries
{
    private readonly IAsyncDocumentSession _session;

    public ThreadQueries(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task AddTweedToThread(string tweedId, string parentTweedId, string threadId)
    {
        var thread = await _session.LoadAsync<TweedThread>(threadId);
        if (thread is null)
        {
            thread = new TweedThread();
            await _session.StoreAsync(thread);
        }
    }
}