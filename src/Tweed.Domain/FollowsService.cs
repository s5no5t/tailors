using NodaTime;
using Tweed.Domain.Model;

namespace Tweed.Domain;

public interface IFollowsService
{
    Task AddFollower(string leaderId, string followerId, ZonedDateTime createdAt);
    Task RemoveFollower(string leaderId, string followerId);
    Task<List<AppUserFollows.LeaderReference>> GetFollows(string userId);
}

public class FollowsService : IFollowsService
{
    private readonly IAppUserFollowsRepository _appUserFollowsRepository;

    public FollowsService(IAppUserFollowsRepository appUserFollowsRepository)
    {
        _appUserFollowsRepository = appUserFollowsRepository;
    }

    public async Task AddFollower(string leaderId, string followerId, ZonedDateTime createdAt)
    {
        var appUserFollows = await GetOrCreateAppUserFollower(followerId);

        if (appUserFollows.Follows.Any(f => f.LeaderId == leaderId))
            return;

        appUserFollows.Follows.Add(new AppUserFollows.LeaderReference
        {
            LeaderId = leaderId,
            CreatedAt = createdAt
        });
    }

    public async Task RemoveFollower(string leaderId, string followerId)
    {
        var follower = await GetOrCreateAppUserFollower(followerId);
        follower.Follows.RemoveAll(f => f.LeaderId == leaderId);
    }

    public async Task<List<AppUserFollows.LeaderReference>> GetFollows(string followerId)
    {
        var follower = await GetOrCreateAppUserFollower(followerId);
        return follower.Follows;
    }

    private async Task<AppUserFollows> GetOrCreateAppUserFollower(string userId)
    {
        var appUserFollowsId = AppUserFollows.BuildId(userId);
        var appUserFollows = await _appUserFollowsRepository.GetById(appUserFollowsId);
        if (appUserFollows is null)
        {
            appUserFollows = new AppUserFollows
            {
                AppUserId = userId
            };
            await _appUserFollowsRepository.Create(appUserFollows);
        }

        return appUserFollows;
    }
}
