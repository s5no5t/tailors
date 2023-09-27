using OneOf;
using OneOf.Types;
using Tailors.Tweed.Domain;

namespace Tailors.Thread.Domain;

public interface IThreadOfTweedsUseCase
{
    Task<OneOf<List<Tweed.Domain.Tweed>, DomainError>> GetThreadTweedsForTweed(string tweedId);
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

    public async Task<OneOf<List<Tweed.Domain.Tweed>, DomainError>> GetThreadTweedsForTweed(string tweedId)
    {
        var tweed = await _tweedRepository.GetById(tweedId);
        if (tweed is null)
            return new DomainError($"Tweed {tweedId} not found");

        if (tweed.ThreadId is null)
            return new List<Tweed.Domain.Tweed>();

        var thread = await _tweedThreadRepository.GetById(tweed.ThreadId!);
        if (thread is null)
            return new DomainError($"Thread {tweed.ThreadId} not found");

        var tweedPath = thread.FindTweedPath(tweed.Id!);
        var tweedsByIds = await _tweedRepository.GetByIds(tweedPath.Select(t => t.TweedId!));
        var tweeds = tweedsByIds.Values.Select(t => t).ToList();
        return tweeds;
    }

    public async Task<OneOf<Success, DomainError>> AddTweedToThread(string tweedId)
    {
        var tweed = await _tweedRepository.GetById(tweedId);
        if (tweed is null)
            return new DomainError($"Tweed {tweedId} not found");

        var thread = tweed.ThreadId switch
        {
            null => await _tweedThreadRepository.Create(),
            _ => await _tweedThreadRepository.GetById(tweed.ThreadId)
        };
        
        if (thread is null)
            return new DomainError($"Thread {tweed.ThreadId} not found");

        tweed.ThreadId = thread.Id;
        
        var result = thread.AddTweed(tweed);
        return result;
    }
}
