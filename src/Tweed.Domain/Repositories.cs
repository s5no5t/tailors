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
    Task<AppUserLikes?> Get(string appUserLikesId);
    Task Create(AppUserLikes appUserLikes);
    Task<long> GetLikesCounter(string tweedId);
    void IncreaseLikesCounter(string tweedId);
    void DecreaseLikesCounter(string tweedId);
}

public interface ITweedRepository
{
    Task<List<Model.Tweed>> GetTweedsForUser(string userId);
    Task<Model.Tweed?> GetTweedById(string id);
    Task<Model.Tweed> CreateRootTweed(string authorId, string text, ZonedDateTime createdAt);

    Task<Model.Tweed> CreateReplyTweed(string authorId, string text, ZonedDateTime createdAt,
        string parentTweedId);

    Task<Model.Tweed?> Get(string tweedId);
}

public interface ITweedThreadRepository
{
    Task<List<TweedThread.TweedReference>?> GetLeadingTweeds(string threadId, string tweedId);
}
