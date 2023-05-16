using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Tweed.Domain;
using Tweed.Domain.Model;
using Tweed.Infrastructure.Indexes;

namespace Tweed.Infrastructure;

public sealed class TweedRepository : ITweedRepository
{
    private readonly IAsyncDocumentSession _session;

    public TweedRepository(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public Task<TheTweed?> GetById(string id)
    {
        return _session.LoadAsync<TheTweed>(id)!;
    }

    public Task<Dictionary<string, TheTweed>> GetByIds(IEnumerable<string> ids)
    {
        return _session.LoadAsync<TheTweed>(ids);
    }

    public async Task Create(TheTweed theTweed)
    {
        await _session.StoreAsync(theTweed);
    }

    public async Task<List<TheTweed>> GetAllByAuthorId(string authorId, int count)
    {
        return await _session.Query<TheTweed, Tweeds_ByAuthorIdAndCreatedAt>()
            .Where(t => t.AuthorId == authorId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .Include(t => t.AuthorId).ToListAsync();
    }

    public async Task<List<TheTweed>> Search(string term)
    {
        return await _session.Query<TheTweed, Tweeds_ByText>()
            .Search(t => t.Text, term)
            .Take(20).ToListAsync();
    }

    public async Task<List<TheTweed>> GetFollowerTweeds(List<string> followedUserIds,
        int count)
    {
        var followerTweeds = await _session
            .Query<TheTweed, Tweeds_ByAuthorIdAndCreatedAt>()
            .Where(t => t.AuthorId.In(followedUserIds))
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .Include(t => t.AuthorId)
            .ToListAsync();
        return followerTweeds;
    }

    public async Task<List<TheTweed>> GetRecentTweeds(int count)
    {
        return await _session.Query<TheTweed>()
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .Include(t => t.AuthorId)
            .ToListAsync();
    }
}