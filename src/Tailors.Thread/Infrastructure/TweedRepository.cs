using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Tailors.Thread.Domain;
using Tailors.Thread.Domain.TweedAggregate;
using Tailors.Thread.Infrastructure.Indexes;

namespace Tailors.Thread.Infrastructure;

public sealed class TweedRepository : ITweedRepository
{
    private readonly IAsyncDocumentSession _session;

    public TweedRepository(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public Task<Tweed?> GetById(string id)
    {
        return _session.LoadAsync<Tweed>(id)!;
    }

    public Task<Dictionary<string, Tweed>> GetByIds(IEnumerable<string> ids)
    {
        return _session.LoadAsync<Tweed>(ids);
    }

    public async Task Create(Tweed tweed)
    {
        await _session.StoreAsync(tweed);
    }

    public async Task<List<Tweed>> GetAllByAuthorId(string authorId, int count)
    {
        return await _session.Query<Tweed, Tweeds_ByAuthorIdAndCreatedAt>()
            .Where(t => t.AuthorId == authorId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .Include(t => t.AuthorId).ToListAsync();
    }

    public async Task<List<Tweed>> Search(string term)
    {
        return await _session.Query<Tweed, Tweeds_ByText>()
            .Search(t => t.Text, term)
            .Take(20).ToListAsync();
    }

    public async Task<List<Tweed>> GetFollowerTweeds(List<string> followedUserIds,
        int count)
    {
        var followerTweeds = await _session
            .Query<Tweed, Tweeds_ByAuthorIdAndCreatedAt>()
            .Where(t => t.AuthorId.In(followedUserIds))
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .Include(t => t.AuthorId)
            .ToListAsync();
        return followerTweeds;
    }

    public async Task<List<Tweed>> GetRecentTweeds(int count)
    {
        return await _session.Query<Tweed>()
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .Include(t => t.AuthorId)
            .ToListAsync();
    }
}