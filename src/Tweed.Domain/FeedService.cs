namespace Tweed.Domain;

public interface IFeedService
{
    Task<List<Domain.Model.Tweed>> GetFeed(string appUserId, int page, int pageSize);
}

public class FeedService : IFeedService
{
    private readonly ITweedRepository _tweedRepository;
    private readonly IAppUserFollowsRepository _appUserFollowsRepository;

    public FeedService(ITweedRepository tweedRepository, IAppUserFollowsRepository appUserFollowsRepository)
    {
        _tweedRepository = tweedRepository;
        _appUserFollowsRepository = appUserFollowsRepository;
    }
    
    public async Task<List<Model.Tweed>> GetFeed(string appUserId, int page, int pageSize)
    {
        const int feedSize = 100;
        
        var ownTweeds = await _tweedRepository.GetTweedsForAuthorId(appUserId, feedSize);

        var follows = await _appUserFollowsRepository.GetFollows(appUserId);
        var followedUserIds = follows.Select(f => f.LeaderId).ToList();

        var followerTweeds = await _tweedRepository.GetFollowerTweeds(followedUserIds, feedSize);

        var numExtraTweeds = feedSize - ownTweeds.Count - followerTweeds.Count;
        var extraTweeds = await _tweedRepository.GetExtraTweeds(ownTweeds, followerTweeds, numExtraTweeds);

        var tweeds = new List<Domain.Model.Tweed>();
        tweeds.AddRange(ownTweeds);
        tweeds.AddRange(followerTweeds);
        tweeds.AddRange(extraTweeds);
        tweeds = tweeds.OrderByDescending(t => t.CreatedAt?.LocalDateTime).ToList();

        var feed = tweeds.Take(pageSize).ToList();

        return feed;
    }
}