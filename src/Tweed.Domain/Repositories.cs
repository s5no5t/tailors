using Tweed.Domain.Model;

namespace Tweed.Domain;

public interface IAppUserRepository
{
    Task<List<AppUser>> Search(string term);
}

public interface IAppUserFollowsRepository
{
    Task<AppUserFollows?> GetById(string appUserFollowsId);
    Task Create(AppUserFollows appUserFollows);
    Task<int> GetFollowerCount(string userId);
}

public interface ITweedLikesRepository
{
    Task<AppUserLikes?> GetById(string appUserLikesId);
    Task Create(AppUserLikes appUserLikes);
    Task<long> GetLikesCounter(string tweedId);
    void IncreaseLikesCounter(string tweedId);
    void DecreaseLikesCounter(string tweedId);
}

public interface ITweedRepository
{
    Task<Model.Tweed?> GetById(string id);
    Task<List<Model.Tweed>> GetAllByAuthorId(string authorId, int count = 20);
    Task Create(Model.Tweed tweed);
    Task<List<Model.Tweed>> Search(string term);
    Task<List<Model.Tweed>> GetRecentTweeds(List<string> ignoreTweedIds, int count);
    Task<List<Model.Tweed>> GetFollowerTweeds(List<string> followedUserIds, int count);
}

public interface ITweedThreadRepository
{
    Task<TweedThread?> GetById(string threadId);
    Task Create(TweedThread thread);
}
