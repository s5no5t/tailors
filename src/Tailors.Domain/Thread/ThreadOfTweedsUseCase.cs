using OneOf;
using OneOf.Types;
using Tailors.Domain.Tweed;

namespace Tailors.Domain.Thread;

public interface IThreadOfTweedsUseCase
{
    Task<OneOf<List<TailorsTweed>, TweedError>> GetThreadTweedsForTweed(string tweedId);
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

    public async Task<OneOf<List<TailorsTweed>, TweedError>> GetThreadTweedsForTweed(string tweedId)
    {
        var tweed = await _tweedRepository.GetById(tweedId);
        if (tweed is null)
            return new TweedError($"Tweed {tweedId} not found");

        if (tweed.ThreadId is null)
            return new List<TailorsTweed>();

        var thread = await _threadRepository.GetById(tweed.ThreadId!);
        if (thread is null)
            return new TweedError($"Thread {tweed.ThreadId} not found");

        var tweedPath = thread.FindTweedPath(tweed.Id!);
        var tweedsByIds = await _tweedRepository.GetByIds(tweedPath.Select(t => t.TweedId!));
        var tweeds = tweedsByIds.Values.Select(t => t).ToList();
        return tweeds;
    }

    public async Task<OneOf<Success, ThreadError, TweedError>> AddTweedToThread(string tweedId)
    {
        var tweed = await _tweedRepository.GetById(tweedId);
        if (tweed is null)
            return new TweedError($"Tweed {tweedId} not found");

        TailorsThread? thread;
        switch (tweed.ThreadId)
        {
            case null:
                thread = new TailorsThread();
                await _threadRepository.Create(thread);
                tweed.ThreadId = thread.Id;
                break;
            default:
                thread = await _threadRepository.GetById(tweed.ThreadId);
                if (thread is null)
                    return new ThreadError($"Thread {tweed.ThreadId} not found");
                break;
        }

        var result = thread.AddTweed(tweed);
        return result;
    }
}
