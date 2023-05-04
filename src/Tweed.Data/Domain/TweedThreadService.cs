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
            AddReplyToThread(thread, tweedId, parentTweedId);
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

    private void AddReplyToThread(TweedThread thread, string tweedId, string parentTweedId)
    {
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
}