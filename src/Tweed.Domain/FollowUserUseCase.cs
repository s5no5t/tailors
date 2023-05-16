using NodaTime;
using Tweed.Domain.Model;

namespace Tweed.Domain;

public interface IFollowUserUseCase
{
    Task AddFollower(string leaderId, string followerId, ZonedDateTime createdAt);
    Task RemoveFollower(string leaderId, string followerId);
    Task<List<UserFollows.LeaderReference>> GetFollows(string userId);
}

public class FollowUserUseCase : IFollowUserUseCase
{
    private readonly IUserFollowsRepository _userFollowsRepository;

    public FollowUserUseCase(IUserFollowsRepository userFollowsRepository)
    {
        _userFollowsRepository = userFollowsRepository;
    }

    public async Task AddFollower(string leaderId, string followerId, ZonedDateTime createdAt)
    {
        var userFollows= await GetOrCreateUserFollower(followerId);

        if (userFollows.Follows.Any(f => f.LeaderId == leaderId))
            return;

        userFollows.Follows.Add(new UserFollows.LeaderReference
        {
            LeaderId = leaderId,
            CreatedAt = createdAt
        });
    }

    public async Task RemoveFollower(string leaderId, string followerId)
    {
        var follower = await GetOrCreateUserFollower(followerId);
        follower.Follows.RemoveAll(f => f.LeaderId == leaderId);
    }

    public async Task<List<UserFollows.LeaderReference>> GetFollows(string followerId)
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
            userFollows = new UserFollows
            {
                UserId = userId
            };
            await _userFollowsRepository.Create(userFollows);
        }

        return userFollows;
    }
}
