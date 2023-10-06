using OneOf;
using OneOf.Types;
using Raven.Client.Documents.Session;
using Tailors.Domain.ThreadAggregate;

namespace Tailors.Infrastructure.ThreadAggregate;

public class ThreadRepository : IThreadRepository
{
    private readonly IAsyncDocumentSession _session;

    public ThreadRepository(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task Create(TailorsThread thread)
    {
        await _session.StoreAsync(thread);
    }

    public async Task<OneOf<TailorsThread, None>> GetById(string threadId)
    {
        var thread = await _session.LoadAsync<TailorsThread>(threadId);
        return thread is null ? new None() : thread;
    }
}
