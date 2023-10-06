namespace Tailors.Domain.UserFollowsAggregate;

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
        var userFollows = await GetOrCreateUserFollower(followerId);

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
        var getUserFollowsResult = await _userFollowsRepository.GetById(userFollowsId);
        if (getUserFollowsResult.TryPickT0(out var existingUserFollows, out _))
            return existingUserFollows;

        var userFollows = new UserFollows(userId);
        await _userFollowsRepository.Create(userFollows);
        return userFollows;
    }
}
