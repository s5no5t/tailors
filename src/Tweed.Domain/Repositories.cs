using Tweed.Domain.Model;

namespace Tweed.Domain;

public interface IUserRepository
{
    Task<List<User>> Search(string term);
}

public interface IUserFollowsRepository
{
    Task<UserFollows?> GetById(string userFollowsId);
    Task Create(UserFollows userFollows);
    Task<int> GetFollowerCount(string userId);
}

public interface ITweedRepository
{
    Task<Model.Tweed?> GetById(string id);
    Task<Dictionary<string, Model.Tweed>> GetByIds(IEnumerable<string> ids);
    Task<List<Model.Tweed>> GetAllByAuthorId(string authorId, int count);
    Task Create(Model.Tweed tweed);
    Task<List<Model.Tweed>> Search(string term);
    Task<List<Model.Tweed>> GetRecentTweeds(int count);
    Task<List<Model.Tweed>> GetFollowerTweeds(List<string> followedUserIds, int count);
}

public interface ITweedThreadRepository
{
    Task<TweedThread?> GetById(string threadId);
    Task Create(TweedThread thread);
}
