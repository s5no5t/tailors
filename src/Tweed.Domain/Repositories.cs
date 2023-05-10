using NodaTime;
using Tweed.Domain.Model;

namespace Tweed.Domain;

public interface IAppUserFollowsService
{
    Task AddFollower(string leaderId, string followerId, ZonedDateTime createdAt);
    Task RemoveFollower(string leaderId, string followerId);
    Task<int> GetFollowerCount(string userId);
    Task<List<AppUserFollows.LeaderReference>> GetFollows(string userId);
}

public interface IFeedService
{
    Task<List<Domain.Model.Tweed>> GetFeed(string appUserId, int page);
}

public interface ISearchService
{
    Task<List<AppUser>> SearchAppUsers(string term);
    Task<List<Domain.Model.Tweed>> SearchTweeds(string term);
}

public interface ITweedLikesService
{
    Task AddLike(string tweedId, string userId, ZonedDateTime likedAt);
    Task RemoveLike(string tweedId, string userId);
    Task<long> GetLikesCount(string tweedId);
    Task<bool> DoesUserLikeTweed(string tweedId, string userId);
}

public interface ITweedService
{
    Task<List<Domain.Model.Tweed>> GetTweedsForUser(string userId);
    Task<Domain.Model.Tweed?> GetTweedById(string id);
    Task<Domain.Model.Tweed> CreateRootTweed(string authorId, string text, ZonedDateTime createdAt);
    Task<Domain.Model.Tweed> CreateReplyTweed(string authorId, string text, ZonedDateTime createdAt,
        string parentTweedId);
}

public interface ITweedThreadService
{
    Task<List<TweedThread.TweedReference>?> GetLeadingTweeds(string threadId, string tweedId);
}
