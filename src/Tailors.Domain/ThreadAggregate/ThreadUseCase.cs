using OneOf.Types;
using OneOf;
using Tailors.Domain.TweedAggregate;

namespace Tailors.Domain.ThreadAggregate;

public interface IThreadOfTweedsUseCase
{
    Task<OneOf<List<Tweed>, ResourceNotFoundError>> GetThreadTweedsForTweed(string tweedId);
}

public class ThreadUseCase : IThreadOfTweedsUseCase
{
    private readonly ITweedRepository _tweedRepository;
    private readonly IThreadRepository _threadRepository;

    public ThreadUseCase(IThreadRepository threadRepository,
        ITweedRepository tweedRepository)
    {
        _threadRepository = threadRepository;
        _tweedRepository = tweedRepository;
    }

    public async Task<OneOf<List<Tweed>, ResourceNotFoundError>> GetThreadTweedsForTweed(string tweedId)
    {
        var getTweedResult = await _tweedRepository.GetById(tweedId);
        if (getTweedResult.TryPickT1(out _, out var tweed))
            return new ResourceNotFoundError($"Tweed {tweedId} not found");

        if (tweed.ThreadId is null)
            return new List<Tweed>();

        var getThreadResult = await _threadRepository.GetById(tweed.ThreadId!);
        if (getThreadResult.TryPickT1(out _, out var thread))
            return new ResourceNotFoundError($"Thread {tweed.ThreadId} not found");

        var tweedPath = thread.FindTweedPath(tweed.Id!);
        var tweedsByIds = await _tweedRepository.GetByIds(tweedPath.Select(t => t.TweedId!));
        var tweeds = tweedsByIds.Values.Select(t => t).ToList();
        return tweeds;
    }

    public async Task<OneOf<Success, ResourceNotFoundError>> AddTweedToThread(string tweedId)
    {
        var getTweedResult = await _tweedRepository.GetById(tweedId);
        if (getTweedResult.TryPickT1(out _, out var tweed))
            return new ResourceNotFoundError($"Tweed {tweedId} not found");

        if (tweed.ThreadId is not null)
            throw new ArgumentException($"Tweed {tweed.Id} already belongs to thread {tweed.ThreadId}");

        if (tweed.ParentTweedId is null)
        {
            await AddToNewThread(tweed);
            return new Success();
        }

        var result = await AddToParentTweedThread(tweed);
        return result;
    }
    
    private async Task AddToNewThread(Tweed tweed)
    {
        TailorsThread newThread = new();
        await _threadRepository.Create(newThread);
        tweed.ThreadId = newThread.Id;
        var result = newThread.AddTweed(tweed);
        result.Switch(_ =>  {}, error => throw new Exception(error.Message));
    }

    private async Task<OneOf<Success, ResourceNotFoundError>> AddToParentTweedThread(Tweed tweed)
    {
        if (tweed.ParentTweedId is null)
            throw new ArgumentException($"Tweed {tweed.Id} is missing ParentTweedId");
        
        var getParentTweedResult = await _tweedRepository.GetById(tweed.ParentTweedId);
        if (getParentTweedResult.TryPickT1(out _, out var parentTweed))
            return new ResourceNotFoundError($"Tweed {tweed.ParentTweedId} not found");

        if (parentTweed.ThreadId is null)
            throw new ArgumentException($"Tweed {parentTweed.Id} does not belong to a thread");
        
        var getParentTweedThreadResult = await _threadRepository.GetById(parentTweed.ThreadId);
        if (getParentTweedThreadResult.TryPickT1(out _, out var parentTweedThread))
            return new ResourceNotFoundError($"Thread {tweed.ThreadId} not found");

        tweed.ThreadId = parentTweed.ThreadId;
        var result = parentTweedThread.AddTweed(tweed);
        if (result.TryPickT1(out _, out _)) // max depth reached
        {
            var childThread = new TailorsThread(parentThreadId: parentTweed.ThreadId);
            await _threadRepository.Create(childThread);
            tweed.ThreadId = childThread.Id;
            childThread.AddTweed(tweed);
            parentTweedThread.AddChildThreadReference(tweed.ParentTweedId, childThread.Id!);
        }

        return new Success();
    }
}
