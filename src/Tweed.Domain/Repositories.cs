using NodaTime;
using Tweed.Domain.Model;

namespace Tweed.Domain;

public interface IAppUserFollowsRepository
{
    Task AddFollower(string leaderId, string followerId, ZonedDateTime createdAt);
    Task RemoveFollower(string leaderId, string followerId);
    Task<int> GetFollowerCount(string userId);
    Task<List<AppUserFollows.LeaderReference>> GetFollows(string userId);
}

public interface ITweedLikesRepository
{
    Task AddLike(string tweedId, string userId, ZonedDateTime likedAt);
    Task RemoveLike(string tweedId, string userId);
    Task<long> GetLikesCount(string tweedId);
    Task<bool> DoesUserLikeTweed(string tweedId, string userId);
}

public interface ITweedRepository
{
    Task<List<Model.Tweed>> GetTweedsForUser(string userId);
    Task<Model.Tweed?> GetTweedById(string id);
    Task<Model.Tweed> CreateRootTweed(string authorId, string text, ZonedDateTime createdAt);

    Task<Model.Tweed> CreateReplyTweed(string authorId, string text, ZonedDateTime createdAt,
        string parentTweedId);
}

public interface ITweedThreadRepository
{
    Task<List<TweedThread.TweedReference>?> GetLeadingTweeds(string threadId, string tweedId);
}
