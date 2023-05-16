using FluentResults;
using Tweed.Domain.Model;

namespace Tweed.Domain;

public interface IThreadOfTweedsUseCase
{
    Task<Result<List<TheTweed>>> GetThreadTweedsForTweed(string tweedId);
}

public class ThreadOfTweedsUseCase : IThreadOfTweedsUseCase
{
    private readonly ITweedRepository _tweedRepository;
    private readonly ITweedThreadRepository _tweedThreadRepository;

    public ThreadOfTweedsUseCase(ITweedThreadRepository tweedThreadRepository,
        ITweedRepository tweedRepository)
    {
        _tweedThreadRepository = tweedThreadRepository;
        _tweedRepository = tweedRepository;
    }

    public async Task<Result<List<TheTweed>>> GetThreadTweedsForTweed(string tweedId)
    {
        var tweed = await _tweedRepository.GetById(tweedId);
        if (tweed is null)
            return Result.Fail($"Tweed {tweedId} not found");

        if (tweed.ThreadId is null)
            return new List<TheTweed>();

        var thread = await _tweedThreadRepository.GetById(tweed.ThreadId!);
        if (thread is null)
            return Result.Fail($"Thread {tweed.ThreadId} not found");

        var path = FindTweedInThread(thread, tweedId);
        if (path is null)
            return Result.Fail($"Tweed {tweedId} not found in Thread {tweed.ThreadId}");

        var tweedsByIds = await _tweedRepository.GetByIds(path.Select(t => t.TweedId!));
        var tweeds = tweedsByIds.Values.Select(t => t).ToList();
        return tweeds;
    }

    public async Task<Result> AddTweedToThread(string tweedId)
    {
        var tweed = await _tweedRepository.GetById(tweedId);
        if (tweed is null)
            return Result.Fail($"Tweed {tweedId} not found");

        if (tweed.ThreadId is null)
            return Result.Fail($"Thread {tweed.ThreadId} is missing ThreadId");
        var thread = await _tweedThreadRepository.GetById(tweed.ThreadId);
        if (thread is null)
            return Result.Fail($"Thread {tweed.ThreadId} not found");

        // This is a root Tweed
        if (tweed.ParentTweedId is null)
        {
            thread!.Root.TweedId = tweedId;
            return Result.Ok();
        }

        // This is a reply to a reply
        var path = FindTweedInThread(thread, tweed.ParentTweedId);
        if (path is null)
            return Result.Fail(
                $"Parent Tweed {tweed.ParentTweedId} not found in Thread {thread.Id}");
        var parentTweedRef = path.Last();
        parentTweedRef.Replies.Add(new TweedThread.TweedReference
        {
            TweedId = tweedId
        });
        return Result.Ok();
    }

    private static List<TweedThread.TweedReference>? FindTweedInThread(TweedThread thread,
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
}