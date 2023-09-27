using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Tailors.Tweed.Domain;
using Tailors.Tweed.Infrastructure.Indexes;

namespace Tailors.Tweed.Infrastructure;

public sealed class TweedRepository : ITweedRepository
{
    private readonly IAsyncDocumentSession _session;

    public TweedRepository(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public Task<Domain.Tweed?> GetById(string id)
    {
        return _session.LoadAsync<Domain.Tweed>(id)!;
    }

    public Task<Dictionary<string, Domain.Tweed>> GetByIds(IEnumerable<string> ids)
    {
        return _session.LoadAsync<Domain.Tweed>(ids);
    }

    public async Task Create(Domain.Tweed tweed)
    {
        await _session.StoreAsync(tweed);
    }

    public async Task<List<Domain.Tweed>> GetAllByAuthorId(string authorId, int count)
    {
        return await _session.Query<Domain.Tweed, Tweeds_ByAuthorIdAndCreatedAt>()
            .Where(t => t.AuthorId == authorId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .Include(t => t.AuthorId).ToListAsync();
    }

    public async Task<List<Domain.Tweed>> Search(string term)
    {
        return await _session.Query<Domain.Tweed, Tweeds_ByText>()
            .Search(t => t.Text, term)
            .Take(20).ToListAsync();
    }

    public async Task<List<Domain.Tweed>> GetFollowerTweeds(List<string> followedUserIds,
        int count)
    {
        var followerTweeds = await _session
            .Query<Domain.Tweed, Tweeds_ByAuthorIdAndCreatedAt>()
            .Where(t => t.AuthorId.In(followedUserIds))
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .Include(t => t.AuthorId)
            .ToListAsync();
        return followerTweeds;
    }

    public async Task<List<Domain.Tweed>> GetRecentTweeds(int count)
    {
        return await _session.Query<Domain.Tweed>()
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .Include(t => t.AuthorId)
            .ToListAsync();
    }
}