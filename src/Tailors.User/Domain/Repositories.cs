namespace Tailors.User.Domain;

public interface IUserRepository
{
    Task<List<AppUser>> Search(string term);
}

public interface IUserFollowsRepository
{
    Task<UserFollows?> GetById(string userFollowsId);
    Task Create(UserFollows userFollows);
    Task<int> GetFollowerCount(string userId);
}