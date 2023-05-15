using Tweed.Domain.Model;

namespace Tweed.Domain;

public interface IThreadService
{
    Task<List<TweedThread.TweedReference>?> GetLeadingTweeds(string threadId, string tweedId);
}

public class ThreadService : IThreadService
{
    private readonly ITweedThreadRepository _tweedThreadRepository;

    public ThreadService(ITweedThreadRepository tweedThreadRepository)
    {
        _tweedThreadRepository = tweedThreadRepository;
    }

    public async Task<List<TweedThread.TweedReference>?> GetLeadingTweeds(string threadId,
        string tweedId)
    {
        var thread = await GetOrCreateThread(threadId);

        var path = FindTweedInThread(thread, tweedId);
        if (path is null)
            return null;

        path.RemoveAt(path.Count - 1);
        return path;
    }

    public async Task AddTweedToThread(string threadId, string tweedId, string? parentTweedId)
    {
        var thread = await GetOrCreateThread(threadId);

        // This is a root Tweed
        if (parentTweedId is null)
        {
            thread.Root.TweedId = tweedId;
            return;
        }

        // This is a reply to a reply
        var path = FindTweedInThread(thread, parentTweedId);
        if (path is null)
            throw new Exception($"Parent Tweed {parentTweedId} not found in Thread {thread.Id}");
        var parentTweedRef = path.Last();
        parentTweedRef.Replies.Add(new TweedThread.TweedReference
        {
            TweedId = tweedId
        });
    }

    private List<TweedThread.TweedReference>? FindTweedInThread(TweedThread thread,
        string tweedId)
    {
        Queue<List<TweedThread.TweedReference>> queue = new();
        queue.Enqueue(new List<TweedThread.TweedReference>
        {
            thread.Root
        });

        while (queue.Count > 0)
        {
            var currentPath = queue.Dequeue();
            var currentRef = currentPath.Last();

            if (currentRef.TweedId == tweedId)
                return currentPath;

            foreach (var reply in currentRef.Replies)
            {
                var replyPath = new List<TweedThread.TweedReference>(currentPath) { reply };
                queue.Enqueue(replyPath);
            }
        }

        return null;
    }

    private async Task<TweedThread> GetOrCreateThread(string threadId)
    {
        var tweedThread = await _tweedThreadRepository.GetById(threadId);
        if (tweedThread is not null) return tweedThread;

        tweedThread = new TweedThread();
        await _tweedThreadRepository.Create(tweedThread);
        return tweedThread;
    }
}
