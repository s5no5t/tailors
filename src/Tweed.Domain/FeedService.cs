namespace Tweed.Domain;

public interface IFeedService
{
    Task<List<Model.Tweed>> GetFeed(string userId, int page, int pageSize);
}

public class FeedService : IFeedService
{
    private readonly IFollowsService _followsService;
    private readonly ITweedRepository _tweedRepository;

    public FeedService(ITweedRepository tweedRepository, IFollowsService followsService)
    {
        _tweedRepository = tweedRepository;
        _followsService = followsService;
    }

    public async Task<List<Model.Tweed>> GetFeed(string userId, int page, int pageSize)
    {
        const int feedSize = 100;

        var ownTweeds = await _tweedRepository.GetAllByAuthorId(userId, feedSize);

        var follows = await _followsService.GetFollows(userId);
        var followedUserIds = follows.Select(f => f.LeaderId).ToList();
        var followerTweeds = await _tweedRepository.GetFollowerTweeds(followedUserIds, feedSize);

        var numExtraTweeds = feedSize - ownTweeds.Count - followerTweeds.Count;
        var tweedsToIgnore = ownTweeds.Select(t => t.Id!).Concat(followerTweeds.Select(t => t.Id!))
            .ToList();
        var extraTweeds = await _tweedRepository.GetRecentTweeds(tweedsToIgnore, numExtraTweeds);

        var tweeds = new List<Model.Tweed>();
        tweeds.AddRange(ownTweeds);
        tweeds.AddRange(followerTweeds);
        tweeds.AddRange(extraTweeds);
        tweeds = tweeds.OrderByDescending(t => t.CreatedAt?.LocalDateTime).ToList();

        var feed = tweeds.Take(pageSize).ToList();

        return feed;
    }
}
