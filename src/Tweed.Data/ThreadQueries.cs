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

    public async Task AddReplyToThread(string tweedId, string parentTweedId, string threadId)
    {
        var thread = await LoadOrCreateThread(threadId);

        if (thread.Root.TweedId is null)
        {
            thread.Root.TweedId = parentTweedId;
            thread.Root.Replies.Add(new TweedReference
            {
                TweedId = tweedId
            });
            return;
        }

        var threadContainsTweed = thread.Root.TweedId == tweedId ||
                                  thread.Root.Replies.Any(r => r.TweedId == tweedId);
        if (!threadContainsTweed) thread.Root.Replies.Add(new TweedReference { TweedId = tweedId });
    }

    private async Task<TweedThread> LoadOrCreateThread(string threadId)
    {
        var thread = await _session.LoadAsync<TweedThread>(threadId);
        if (thread is not null) return thread;

        var newThread = new TweedThread
        {
            Id = threadId
        };
        await _session.StoreAsync(newThread);
        return newThread;
    }
}
