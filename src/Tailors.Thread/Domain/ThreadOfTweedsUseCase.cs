using OneOf;
using OneOf.Types;
using Tailors.Tweed.Domain;

namespace Tailors.Thread.Domain;

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

        var thread = tweed.ThreadId switch
        {
            null => await _threadRepository.Create(),
            _ => await _threadRepository.GetById(tweed.ThreadId)
        };
        
        if (thread is null)
            return new ThreadError($"Thread {tweed.ThreadId} not found");

        tweed.ThreadId = thread.Id;
        
        var result = thread.AddTweed(tweed);
        return result;
    }
}
