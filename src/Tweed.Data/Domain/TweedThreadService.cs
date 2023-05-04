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
            thread.Root.Replies.Add(new TweedThread.TweedReference
            {
                TweedId = tweedId
            });
            return;
        }

        // This is a reply to the root Tweed
        if (thread.Root.TweedId == parentTweedId)
        {
            thread.Root.Replies.Add(new TweedThread.TweedReference
            {
                TweedId = tweedId
            });
            return;
        }

        // This is a reply to a reply
        var parentTweedReference = FindTweedReference(thread.Root, parentTweedId);
        if (parentTweedReference is null)
            thread.Root.Replies.Add(new TweedThread.TweedReference
            {
                TweedId = parentTweedId,
                Replies = new List<TweedThread.TweedReference>
                {
                    new()
                    {
                        TweedId = tweedId
                    }
                }
            });
        else
            parentTweedReference.Replies.Add(new TweedThread.TweedReference
            {
                TweedId = tweedId
            });
    }

    private TweedThread.TweedReference? FindTweedReference(TweedThread.TweedReference currentReference, string tweedId)
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

    public async Task<TweedThread> InsertTweedIntoThread(string tweedId, string? parentTweedId)
    {
        if (parentTweedId is null)
        {
            var thread = await CreateThreadForTweed(tweedId);
            return thread;
        }
        else
        {
            var thread = await FindThreadForTweed(parentTweedId);
            await AddReplyToThread(thread.Id!, tweedId, parentTweedId);
            return thread;
        }
    }

    private async Task<TweedThread> CreateThreadForTweed(string rootTweedId)
    {
        TweedThread thread = new()
        {
            Root = new TweedThread.TweedReference
            {
                TweedId = rootTweedId
            }
        };
        await _session.StoreAsync(thread);
        return thread;
    }

    private Task<TweedThread> FindThreadForTweed(string tweedId)
    {
        throw new NotImplementedException();
    }
}