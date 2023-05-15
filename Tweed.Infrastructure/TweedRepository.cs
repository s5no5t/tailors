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

    public async Task<List<Domain.Model.Tweed>> GetTweedsForUser(string userId)
    {
        return await _session.Query<Domain.Model.Tweed, Tweeds_ByAuthorIdAndCreatedAt>()
            .Where(t => t.AuthorId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(20)
            .ToListAsync();
    }

    public Task<Domain.Model.Tweed?> GetById(string id)
    {
        return _session.LoadAsync<Domain.Model.Tweed>(id)!;
    }

    public async Task Create(Domain.Model.Tweed tweed)
    {
        await _session.StoreAsync(tweed);
    }

    public async Task<List<Domain.Model.Tweed>> SearchTweeds(string term)
    {
        return await _session.Query<Domain.Model.Tweed, Tweeds_ByText>()
            .Search(t => t.Text, term)
            .Take(20).ToListAsync();
    }

    public async Task<List<Domain.Model.Tweed>> GetExtraTweeds(List<Domain.Model.Tweed> ownTweeds, List<Domain.Model.Tweed> followerTweeds, int count)
    {
        var extraTweeds = await _session.Query<Domain.Model.Tweed>()
            .Where(t =>
                !t.Id.In(ownTweeds.Select(f => f.Id)
                    .ToList())) // not my own Tweeds
            .Where(t =>
                !t.Id.In(followerTweeds.Select(f => f.Id)
                    .ToList())) // not Tweeds from users I follow
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .Include(t => t.AuthorId)
            .ToListAsync();
        return extraTweeds;
    }

    public async Task<List<Domain.Model.Tweed>> GetFollowerTweeds(List<string?> followedUserIds, int count)
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

    public async Task<List<Domain.Model.Tweed>> GetTweedsForAuthorId(string authorId, int count)
    {
        var ownTweeds = await _session.Query<Domain.Model.Tweed>()
            .Where(t => t.AuthorId == authorId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .Include(t => t.AuthorId)
            .ToListAsync();
        return ownTweeds;
    }
}
