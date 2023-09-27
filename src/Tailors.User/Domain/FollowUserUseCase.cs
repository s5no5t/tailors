namespace Tailors.User.Domain;

public interface IFollowUserUseCase
{
    Task AddFollower(string leaderId, string followerId, DateTime createdAt);
    Task RemoveFollower(string leaderId, string followerId);
    Task<IReadOnlyList<UserFollows.LeaderReference>> GetFollows(string userId);
}

public class FollowUserUseCase : IFollowUserUseCase
{
    private readonly IUserFollowsRepository _userFollowsRepository;

    public FollowUserUseCase(IUserFollowsRepository userFollowsRepository)
    {
        _userFollowsRepository = userFollowsRepository;
    }

    public async Task AddFollower(string leaderId, string followerId, DateTime createdAt)
    {
        var userFollows= await GetOrCreateUserFollower(followerId);

        userFollows.AddFollows(leaderId, createdAt);
    }

    public async Task RemoveFollower(string leaderId, string followerId)
    {
        var follower = await GetOrCreateUserFollower(followerId);
        follower.RemoveFollows(leaderId);
    }

    public async Task<IReadOnlyList<UserFollows.LeaderReference>> GetFollows(string followerId)
    {
        var follower = await GetOrCreateUserFollower(followerId);
        return follower.Follows;
    }

    private async Task<UserFollows> GetOrCreateUserFollower(string userId)
    {
        var userFollowsId = UserFollows.BuildId(userId);
        var userFollows = await _userFollowsRepository.GetById(userFollowsId);
        if (userFollows is null)
        {
            userFollows = new UserFollows(userId: userId);
            await _userFollowsRepository.Create(userFollows);
        }

        return userFollows;
    }
}
