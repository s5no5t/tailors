namespace Tailors.Domain.UserFollows;

public interface IUserFollowsRepository
{
    Task<UserFollows?> GetById(string userFollowsId);
    Task Create(UserFollows userFollows);
    Task<int> GetFollowerCount(string userId);
}