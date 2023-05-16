using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Tweed.Domain;
using Tweed.Infrastructure.Indexes;

namespace Tweed.Infrastructure;

public sealed class TweedRepository : ITweedRepository
{
    private readonly IAsyncDocumentSession _session;

    public TweedRepository(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public Task<Domain.Model.Tweed?> GetById(string id)
    {
        return _session.LoadAsync<Domain.Model.Tweed>(id)!;
    }

    public Task<Dictionary<string, Domain.Model.Tweed>> GetByIds(IEnumerable<string> ids)
    {
        return _session.LoadAsync<Domain.Model.Tweed>(ids);
    }

    public async Task Create(Domain.Model.Tweed tweed)
    {
        await _session.StoreAsync(tweed);
    }

    public async Task<List<Domain.Model.Tweed>> GetAllByAuthorId(string authorId, int count)
    {
        return await _session.Query<Domain.Model.Tweed, Tweeds_ByAuthorIdAndCreatedAt>()
            .Where(t => t.AuthorId == authorId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .Include(t => t.AuthorId).ToListAsync();
    }

    public async Task<List<Domain.Model.Tweed>> Search(string term)
    {
        return await _session.Query<Domain.Model.Tweed, Tweeds_ByText>()
            .Search(t => t.Text, term)
            .Take(20).ToListAsync();
    }

    public async Task<List<Domain.Model.Tweed>> GetFollowerTweeds(List<string> followedUserIds,
        int count)
    {
        var followerTweeds = await _session
            .Query<Domain.Model.Tweed, Tweeds_ByAuthorIdAndCreatedAt>()
            .Where(t => t.AuthorId.In(followedUserIds))
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .Include(t => t.AuthorId)
            .ToListAsync();
        return followerTweeds;
    }

    public async Task<List<Domain.Model.Tweed>> GetRecentTweeds(int count)
    {
        return await _session.Query<Domain.Model.Tweed>()
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .Include(t => t.AuthorId)
            .ToListAsync();
    }
}
