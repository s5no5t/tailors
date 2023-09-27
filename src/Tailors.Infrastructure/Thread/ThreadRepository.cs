using Raven.Client.Documents.Session;
using Tailors.Thread.Domain;

namespace Tailors.Thread.Infrastructure;

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

    public async Task<TailorsThread?> GetById(string threadId)
    {
        return await _session.LoadAsync<TailorsThread>(threadId);
    }
}