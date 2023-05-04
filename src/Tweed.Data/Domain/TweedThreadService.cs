using Raven.Client.Documents.Session;
using Tweed.Data.Model;

namespace Tweed.Data.Domain;

public class TweedThreadService
{
    private readonly IAsyncDocumentSession _session;

    public TweedThreadService(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task AddReplyToThread(string threadId, string tweedId, string parentTweedId)
    {
        var thread = await LoadOrCreateThread(threadId);

        // There is no Thread yet
        if (thread.Root.TweedId is null)
        {
            thread.Root.TweedId = parentTweedId;
            thread.Root.Replies.Add(new TweedReference
            {
                TweedId = tweedId
            });
            return;
        }

        // This is a reply to the root Tweed
        if (thread.Root.TweedId == parentTweedId)
        {
            thread.Root.Replies.Add(new TweedReference
            {
                TweedId = tweedId
            });
            return;
        }

        // This is a reply to a reply
        var parentTweedReference = FindTweedReference(thread.Root, parentTweedId);
        if (parentTweedReference is null)
            thread.Root.Replies.Add(new TweedReference
            {
                TweedId = parentTweedId,
                Replies = new List<TweedReference>
                {
                    new()
                    {
                        TweedId = tweedId
                    }
                }
            });
        else
            parentTweedReference.Replies.Add(new TweedReference
            {
                TweedId = tweedId
            });
    }

    private TweedReference? FindTweedReference(TweedReference currentReference, string tweedId)
    {
        if (currentReference.TweedId == tweedId) return currentReference;

        foreach (var reply in currentReference.Replies)
        {
            var reference = FindTweedReference(reply, tweedId);
            if (reference is not null)
                return reference;
        }

        return null;
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

    public async Task StoreThread(TweedThread thread)
    {
        await _session.StoreAsync(thread);
    }

    public async Task<TweedThread> FindOrCreateThreadForTweed(string tweedId)
    {
        throw new NotImplementedException();
    }
}
