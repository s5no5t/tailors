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