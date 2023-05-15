using Tweed.Domain.Model;

namespace Tweed.Domain;

public interface IAppUserRepository
{
    Task<List<AppUser>> SearchAppUsers(string term);
}

public interface IAppUserFollowsRepository
{
    Task<int> GetFollowerCount(string userId);
    Task<AppUserFollows?> GetById(string appUserFollowsId);
    Task Create(AppUserFollows appUserFollows);
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

    Task<List<Model.Tweed>> GetExtraTweeds(List<Model.Tweed> ownTweeds,
        List<Model.Tweed> followerTweeds, int count);

    Task<List<Model.Tweed>> GetFollowerTweeds(List<string?> followedUserIds, int count);
    Task<List<Model.Tweed>> GetTweedsForAuthorId(string authorId, int count);
}

public interface ITweedThreadRepository
{
    Task<TweedThread?> GetById(string threadId);
    Task Create(TweedThread thread);
}
