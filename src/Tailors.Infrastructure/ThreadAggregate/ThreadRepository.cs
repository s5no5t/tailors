using OneOf;
using OneOf.Types;
using Raven.Client.Documents.Session;
using Tailors.Domain.ThreadAggregate;

namespace Tailors.Infrastructure.ThreadAggregate;

public class ThreadRepository(IAsyncDocumentSession session) : IThreadRepository
{
    public async Task Create(TailorsThread thread)
    {
        await session.StoreAsync(thread);
    }

    public async Task<OneOf<TailorsThread, None>> GetById(string threadId)
    {
        var thread = await session.LoadAsync<TailorsThread>(threadId);
        return thread is null ? new None() : thread;
    }
}
