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
        string threadId;
        if (tweed.ParentTweedId is null)
        {
            TweedThread thread = new()
            {
                Root =
                {
                    TweedId = tweed.Id
                }
            };
            await _session.StoreAsync(thread);
            threadId = thread.Id!;
        }
        else
        {
            var parentTweed = await _session.LoadAsync<Model.Tweed>(tweed.ParentTweedId);
            threadId = parentTweed.ThreadId!;
        }

        tweed.ThreadId = threadId;
        await _session.StoreAsync(tweed);
    }
}
