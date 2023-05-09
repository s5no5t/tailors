using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Tweed.Domain.Indexes;
using Tweed.Domain.Model;

namespace Tweed.Domain;

public interface ITweedService
{
    Task<List<Model.Tweed>> GetTweedsForUser(string userId);
    Task<Model.Tweed?> GetById(string id);
    Task CreateTweed(Model.Tweed tweed);
}

public sealed class TweedService : ITweedService
{
    private readonly IAsyncDocumentSession _session;

    public TweedService(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task<List<Model.Tweed>> GetTweedsForUser(string userId)
    {
        return await _session.Query<Model.Tweed, Tweeds_ByAuthorIdAndCreatedAt>()
            .Where(t => t.AuthorId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(20)
            .ToListAsync();
    }

    public Task<Model.Tweed?> GetById(string id)
    {
        return _session.LoadAsync<Model.Tweed>(id)!;
    }

    public async Task CreateTweed(Model.Tweed tweed)
    {
        var threadId = tweed.ParentTweedId switch
        {
            null => (await CreateThread(tweed.Id!)).Id,
            not null => (await _session.LoadAsync<Model.Tweed>(tweed.ParentTweedId)).ThreadId
        };
        tweed.ThreadId = threadId;

        await _session.StoreAsync(tweed);
    }

    private async Task<TweedThread> CreateThread(string tweedId)
    {
        TweedThread thread = new()
        {
            Root =
            {
                TweedId = tweedId
            }
        };
        await _session.StoreAsync(thread);
        return thread;
    }
}
