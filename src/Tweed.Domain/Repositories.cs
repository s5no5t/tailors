using NodaTime;
using Tweed.Domain.Model;

namespace Tweed.Domain;

public interface IAppUserRepository
{
    Task<List<AppUser>> SearchAppUsers(string term);
}

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
    Task<List<Model.Tweed>> SearchTweeds(string term);
    Task Create(Model.Tweed tweed);
}

public interface ITweedThreadRepository
{
    Task<List<TweedThread.TweedReference>?> GetLeadingTweeds(string threadId, string tweedId);
    Task Create(TweedThread thread);
}