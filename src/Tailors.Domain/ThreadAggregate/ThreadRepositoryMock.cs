using OneOf;
using OneOf.Types;

namespace Tailors.Domain.ThreadAggregate;

public class ThreadRepositoryMock : IThreadRepository
{
    private readonly Random _random = new();
    private readonly Dictionary<string, TailorsThread> _threads = new();

    public Task<OneOf<TailorsThread, None>> GetById(string threadId)
    {
        _threads.TryGetValue(threadId, out var thread);

        if (thread is not null)
            return Task.FromResult<OneOf<TailorsThread, None>>(thread);

        return Task.FromResult<OneOf<TailorsThread, None>>(new None());
    }

    public Task Create(TailorsThread thread)
    {
        thread.Id ??= _random.Next().ToString();
        _threads.Add(thread.Id, thread);
        return Task.CompletedTask;
    }
}
