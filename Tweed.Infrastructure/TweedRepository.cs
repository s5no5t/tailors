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

    public async Task Create(Domain.Model.Tweed tweed)
    {
        await _session.StoreAsync(tweed);
    }

    public async Task<List<Domain.Model.Tweed>> GetAllByAuthorId(string authorId, int count = 20)
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

    public async Task<List<Domain.Model.Tweed>> GetFollowerTweeds(List<string?> followedUserIds,
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

    public async Task<List<Domain.Model.Tweed>> GetRecentTweeds(List<string> ignoreTweedIds,
        int count)
    {
        return await _session.Query<Domain.Model.Tweed>()
            .Where(t =>
                !t.Id.In(ignoreTweedIds)) // not my own Tweeds
            .Where(t =>
                !t.Id.In(ignoreTweedIds)) // not Tweeds from users I follow
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .Include(t => t.AuthorId)
            .ToListAsync();
    }
}
