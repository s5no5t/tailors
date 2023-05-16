using FluentResults;
using Tweed.Domain.Model;

namespace Tweed.Domain;

public interface IShowThreadUseCase
{
    Task<Result<List<Model.Tweed>>> GetThreadTweedsForTweed(string tweedId);
}

public class ShowThreadUseCase : IShowThreadUseCase
{
    private readonly ITweedRepository _tweedRepository;
    private readonly ITweedThreadRepository _tweedThreadRepository;

    public ShowThreadUseCase(ITweedThreadRepository tweedThreadRepository,
        ITweedRepository tweedRepository)
    {
        _tweedThreadRepository = tweedThreadRepository;
        _tweedRepository = tweedRepository;
    }

    public async Task<Result<List<Model.Tweed>>> GetThreadTweedsForTweed(string tweedId)
    {
        var tweed = await _tweedRepository.GetById(tweedId);
        if (tweed is null)
            return Result.Fail($"Tweed {tweedId} not found");

        if (tweed.ThreadId is null)
            return new List<Model.Tweed>();

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

        var thread = await GetOrCreateThreadForTweed(tweed);

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

    private async Task<TweedThread> GetOrCreateThreadForTweed(Model.Tweed tweed)
    {
        if (tweed.ThreadId is null)
        {
            var thread = new TweedThread();
            await _tweedThreadRepository.Create(thread);
            tweed.ThreadId = thread.Id;
            return thread;
        }
        else
        {
            var thread = await _tweedThreadRepository.GetById(tweed.ThreadId);
            if (thread is null)
            {
                thread = new TweedThread
                {
                    Id = tweed.ThreadId
                };
                await _tweedThreadRepository.Create(thread);
                tweed.ThreadId = thread.Id;
            }

            return thread;
        }
    }
}
