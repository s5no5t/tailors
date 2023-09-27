using Tailors.Domain.Tweed;
using Tailors.Domain.UserFollows;

namespace Tailors.Domain.Thread;

public interface IShowFeedUseCase
{
    Task<List<TailorsTweed>> GetFeed(string userId, int page, int pageSize);
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

    public async Task<List<TailorsTweed>> GetFeed(string userId, int page, int pageSize)
    {
        const int feedSize = 100;

        var ownTweeds = await _tweedRepository.GetAllByAuthorId(userId, feedSize);

        var follows = await _followUserUseCase.GetFollows(userId);
        var followedUserIds = follows.Select(f => f.LeaderId).ToList();
        var followerTweeds = await _tweedRepository.GetFollowerTweeds(followedUserIds, feedSize);

        var numExtraTweeds = feedSize - ownTweeds.Count - followerTweeds.Count;
        var extraTweeds = await _tweedRepository.GetRecentTweeds(numExtraTweeds);

        var tweeds = new List<TailorsTweed>();
        tweeds.AddRange(ownTweeds);
        tweeds.AddRange(followerTweeds);
        tweeds.AddRange(extraTweeds);

        var feed = tweeds
            .DistinctBy(t => t.Id)
            .OrderByDescending(t => t.CreatedAt)
            .Skip(pageSize * page)
            .Take(pageSize)
            .ToList();

        return feed;
    }
}
