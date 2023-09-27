using Raven.Client.Documents.Session;
using Tailors.Thread.Domain.ThreadAggregate;

namespace Tailors.Thread.Infrastructure;

public class TweedThreadRepository : ITweedThreadRepository
{
    private readonly IAsyncDocumentSession _session;

    public TweedThreadRepository(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task<TweedThread> Create()
    {
        TweedThread thread = new();
        await _session.StoreAsync(thread);
        return thread;
    }

    public async Task<TweedThread?> GetById(string threadId)
    {
        return await _session.LoadAsync<TweedThread>(threadId);
    }
}