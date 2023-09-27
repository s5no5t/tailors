using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Tailors.Domain.Tweed;
using Tailors.Infrastructure.Tweed.Indexes;

namespace Tailors.Infrastructure.Tweed;

public sealed class TweedRepository : ITweedRepository
{
    private readonly IAsyncDocumentSession _session;

    public TweedRepository(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public Task<TailorsTweed?> GetById(string id)
    {
        return _session.LoadAsync<TailorsTweed>(id)!;
    }

    public Task<Dictionary<string, TailorsTweed>> GetByIds(IEnumerable<string> ids)
    {
        return _session.LoadAsync<TailorsTweed>(ids);
    }

    public async Task Create(TailorsTweed tweed)
    {
        await _session.StoreAsync(tweed);
    }

    public async Task<List<TailorsTweed>> GetAllByAuthorId(string authorId, int count)
    {
        return await _session.Query<TailorsTweed, TweedsByAuthorIdAndCreatedAt>()
            .Where(t => t.AuthorId == authorId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .Include(t => t.AuthorId).ToListAsync();
    }

    public async Task<List<TailorsTweed>> Search(string term)
    {
        return await _session.Query<TailorsTweed, TweedsByText>()
            .Search(t => t.Text, term)
            .Take(20).ToListAsync();
    }

    public async Task<List<TailorsTweed>> GetFollowerTweeds(List<string> followedUserIds,
        int count)
    {
        var followerTweeds = await _session
            .Query<TailorsTweed, TweedsByAuthorIdAndCreatedAt>()
            .Where(t => t.AuthorId.In(followedUserIds))
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .Include(t => t.AuthorId)
            .ToListAsync();
        return followerTweeds;
    }

    public async Task<List<TailorsTweed>> GetRecentTweeds(int count)
    {
        return await _session.Query<TailorsTweed>()
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .Include(t => t.AuthorId)
            .ToListAsync();
    }
}