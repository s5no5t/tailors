using System.Collections.ObjectModel;
using OneOf;
using OneOf.Types;
using Tailors.Thread.Domain.TweedAggregate;

namespace Tailors.Thread.Domain.ThreadAggregate;

public interface IThreadOfTweedsUseCase
{
    Task<OneOf<List<Tweed>, ReferenceNotFoundError>> GetThreadTweedsForTweed(string tweedId);
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

    public async Task<OneOf<List<Tweed>, ReferenceNotFoundError>> GetThreadTweedsForTweed(string tweedId)
    {
        var tweed = await _tweedRepository.GetById(tweedId);
        if (tweed is null)
            return new ReferenceNotFoundError($"Tweed {tweedId} not found");

        if (tweed.ThreadId is null)
            return new List<Tweed>();

        var thread = await _tweedThreadRepository.GetById(tweed.ThreadId!);
        if (thread is null)
            return new ReferenceNotFoundError($"Thread {tweed.ThreadId} not found");

        var path = FindTweedInThread(thread, tweedId);
        var tweedsByIds = await _tweedRepository.GetByIds(path.Select(t => t.TweedId!));
        var tweeds = tweedsByIds.Values.Select(t => t).ToList();
        return tweeds;
    }

    public async Task<OneOf<Success, ReferenceNotFoundError>> AddTweedToThread(string tweedId)
    {
        var tweed = await _tweedRepository.GetById(tweedId);
        if (tweed is null)
            return new ReferenceNotFoundError($"Tweed {tweedId} not found");

        if (tweed.ThreadId is null)
            return new ReferenceNotFoundError($"Thread {tweed.ThreadId} is missing ThreadId");
        var thread = await _tweedThreadRepository.GetById(tweed.ThreadId);
        if (thread is null)
            return new ReferenceNotFoundError($"Thread {tweed.ThreadId} not found");

        var result = thread.AddTweed(tweed);
        return result;
    }

    private static ReadOnlyCollection<TweedThread.TweedReference> FindTweedInThread(TweedThread thread,
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
                return currentPath.AsReadOnly();

            foreach (var reply in currentRef.Replies)
            {
                var replyPath = new List<TweedThread.TweedReference>(currentPath) { reply };
                queue.Enqueue(replyPath);
            }
        }

        return new ReadOnlyCollection<TweedThread.TweedReference>(Array.Empty<TweedThread.TweedReference>());
    }
}
