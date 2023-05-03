using Raven.Client.Documents.Session;

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
        var thread = await _session.LoadAsync<Model.Thread>(threadId);
        if (thread is null)
        {
            thread = new Model.Thread();
            await _session.StoreAsync(thread);
        }
    }
}