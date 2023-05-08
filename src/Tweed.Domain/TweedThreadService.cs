using Raven.Client.Documents.Session;
using Tweed.Domain.Model;

namespace Tweed.Domain;

public interface ITweedThreadService
{
}

public class TweedThreadService : ITweedThreadService
{
    private readonly IAsyncDocumentSession _session;

    public TweedThreadService(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task<TweedThread> LoadThread(string threadId)
    {
        return await _session.LoadAsync<TweedThread>(threadId);
    }

    public void AddTweedToThread(TweedThread thread, string tweedId, string? parentTweedId)
    {
        if (parentTweedId is null)
        {
            thread.Root.TweedId = tweedId;
            return;
        }

        // This is a reply to a reply
        var parentTweedReference = FindTweedReference(thread.Root, parentTweedId);
        if (parentTweedReference is null)
            throw new Exception($"Tweed {parentTweedId} not found in Thread {thread.Id}");
        parentTweedReference.Replies.Add(new TweedThread.TweedReference
        {
            TweedId = tweedId
        });
    }

    private TweedThread.TweedReference? FindTweedReference(
        TweedThread.TweedReference currentReference, string tweedId)
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
