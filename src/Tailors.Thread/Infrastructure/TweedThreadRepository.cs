using Raven.Client.Documents.Session;
using Tailors.Thread.Domain;

namespace Tailors.Thread.Infrastructure;

public class TweedThreadRepository : ITweedThreadRepository
{
    private readonly IAsyncDocumentSession _session;

    public TweedThreadRepository(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task Create(TweedThread thread)
    {
        await _session.StoreAsync(thread);
    }

    public async Task<TweedThread?> GetById(string threadId)
    {
        return await _session.LoadAsync<TweedThread>(threadId);
    }
}