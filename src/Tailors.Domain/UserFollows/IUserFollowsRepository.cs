namespace Tailors.User.Domain.UserFollowsAggregate;

public interface IUserFollowsRepository
{
    Task<UserFollows?> GetById(string userFollowsId);
    Task Create(UserFollows userFollows);
    Task<int> GetFollowerCount(string userId);
}