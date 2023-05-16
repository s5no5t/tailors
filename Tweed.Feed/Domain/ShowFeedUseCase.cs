using Tweed.Thread.Domain;
using Tweed.User;
using Tweed.User.Domain;

namespace Tweed.Feed.Domain;

public interface IShowFeedUseCase
{
    Task<List<Thread.Domain.Tweed>> GetFeed(string userId, int page, int pageSize);
}

public class ShowFeedUseCase : IShowFeedUseCase
{
    private readonly IFollowUserUseCase _followUserUseCase;
    private readonly ITweedRepository _tweedRepository;

    public ShowFeedUseCase(ITweedRepository tweedRepository, IFollowUserUseCase followUserUseCase)
    {
        _tweedRepository = tweedRepository;
        _followUserUseCase = followUserUseCase;
    }

    public async Task<List<Thread.Domain.Tweed>> GetFeed(string userId, int page, int pageSize)
    {
        const int feedSize = 100;

        var ownTweeds = await _tweedRepository.GetAllByAuthorId(userId, feedSize);

        var follows = await _followUserUseCase.GetFollows(userId);
        var followedUserIds = follows.Select(f => f.LeaderId!).ToList();
        var followerTweeds = await _tweedRepository.GetFollowerTweeds(followedUserIds, feedSize);

        var numExtraTweeds = feedSize - ownTweeds.Count - followerTweeds.Count;
        var extraTweeds = await _tweedRepository.GetRecentTweeds(numExtraTweeds);

        var tweeds = new List<Thread.Domain.Tweed>();
        tweeds.AddRange(ownTweeds);
        tweeds.AddRange(followerTweeds);
        tweeds.AddRange(extraTweeds);

        var feed = tweeds
            .DistinctBy(t => t.Id)
            .OrderByDescending(t => t.CreatedAt?.LocalDateTime)
            .Skip(pageSize * page)
            .Take(pageSize)
            .ToList();

        return feed;
    }
}