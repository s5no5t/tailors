using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Tweed.Domain.Indexes;

namespace Tweed.Domain.Domain;

public interface IFeedService
{
    Task<List<Model.Tweed>> GetFeed(string appUserId, int page);
}

public class FeedService : IFeedService
{
    public const int PageSize = 20;
    private const int FeedSize = 100;
    private readonly IAppUserFollowsService _appUserFollowsService;
    private readonly IAsyncDocumentSession _session;

    public FeedService(IAsyncDocumentSession session, IAppUserFollowsService appUserFollowsService)
    {
        _session = session;
        _appUserFollowsService = appUserFollowsService;
    }

    public async Task<List<Model.Tweed>> GetFeed(string appUserId, int page)
    {
        var ownTweeds = await _session.Query<Model.Tweed>()
            .Where(t => t.AuthorId == appUserId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(FeedSize)
            .Include(t => t.AuthorId)
            .ToListAsync();

        var follows = await _appUserFollowsService.GetFollows(appUserId);
        var followedUserIds = follows.Select(f => f.LeaderId).ToList();

        var followerTweeds = await _session.Query<Model.Tweed, Tweeds_ByAuthorIdAndCreatedAt>()
            .Where(t => t.AuthorId.In(followedUserIds))
            .OrderByDescending(t => t.CreatedAt)
            .Take(FeedSize)
            .Include(t => t.AuthorId)
            .ToListAsync();

        var numExtraTweeds = FeedSize - ownTweeds.Count - followerTweeds.Count;
        var extraTweeds = await _session.Query<Model.Tweed>()
            .Where(t =>
                !t.Id.In(ownTweeds.Select(f => f.Id)
                    .ToList())) // not my own Tweeds
            .Where(t =>
                !t.Id.In(followerTweeds.Select(f => f.Id)
                    .ToList())) // not Tweeds from users I follow
            .OrderByDescending(t => t.CreatedAt)
            .Take(numExtraTweeds)
            .Include(t => t.AuthorId)
            .ToListAsync();

        var tweeds = new List<Model.Tweed>();
        tweeds.AddRange(ownTweeds);
        tweeds.AddRange(followerTweeds);
        tweeds.AddRange(extraTweeds);
        tweeds = tweeds.OrderByDescending(t => t.CreatedAt?.LocalDateTime).ToList();

        var feed = tweeds.Take(PageSize).ToList();

        return feed;
    }
}
