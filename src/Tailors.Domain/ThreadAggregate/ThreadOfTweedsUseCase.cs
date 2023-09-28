using OneOf.Types;
using OneOf;
using Tailors.Domain.TweedAggregate;

namespace Tailors.Domain.ThreadAggregate;

public interface IThreadOfTweedsUseCase
{
    Task<OneOf<List<Tweed>, ResourceNotFoundError>> GetThreadTweedsForTweed(string tweedId);
}

public class ThreadOfTweedsUseCase : IThreadOfTweedsUseCase
{
    private readonly ITweedRepository _tweedRepository;
    private readonly IThreadRepository _threadRepository;

    public ThreadOfTweedsUseCase(IThreadRepository threadRepository,
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

        if (tweed.ThreadId == null)
        {
            TailorsThread newThread = new();
            await _threadRepository.Create(newThread);
            tweed.ThreadId = newThread.Id;
            var addTweedResult = newThread.AddTweed(tweed);
            return addTweedResult.Match(s =>  s, error => throw new Exception(error.Message));
        }

        var getThreadResult = await _threadRepository.GetById(tweed.ThreadId!);
        if (getThreadResult.TryPickT1(out _, out var thread))
            return new ResourceNotFoundError($"Thread {tweed.ThreadId} not found");
        
        var result = thread.AddTweed(tweed);
        return result.Match(s =>  s, error => throw new Exception(error.Message));
    }
}
