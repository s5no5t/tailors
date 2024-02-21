using Tailors.Domain.UserFollowsAggregate;

namespace Tailors.Domain.TweedAggregate;

public class ShowFeedUseCase(ITweedRepository tweedRepository, FollowUserUseCase followUserUseCase)
{
    public async Task<List<Tweed>> GetFeed(string userId, int page, int pageSize)
    {
        const int feedSize = 100;

        var ownTweeds = await tweedRepository.GetAllByAuthorId(userId, feedSize);

        var follows = await followUserUseCase.GetFollows(userId);
        var followedUserIds = follows.Select(f => f.LeaderId).ToList();
        var followerTweeds = await tweedRepository.GetFollowerTweeds(followedUserIds, feedSize);

        var numExtraTweeds = feedSize - ownTweeds.Count - followerTweeds.Count;
        var extraTweeds = await tweedRepository.GetRecentTweeds(numExtraTweeds);

        var tweeds = new List<Tweed>();
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
