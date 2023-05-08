using NodaTime;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Tweed.Domain.Indexes;
using Tweed.Domain.Model;

namespace Tweed.Domain.Domain;

public interface IAppUserFollowsService
{
    Task AddFollower(string leaderId, string followerId, ZonedDateTime createdAt);
    Task RemoveFollower(string leaderId, string followerId);
    Task<int> GetFollowerCount(string userId);
    Task<List<AppUserFollows.LeaderReference>> GetFollows(string userId);
}

public class AppUserFollowsService : IAppUserFollowsService
{
    private readonly IAsyncDocumentSession _session;

    public AppUserFollowsService(IAsyncDocumentSession session)
    {
        _session = session;
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

    public async Task<int> GetFollowerCount(string userId)
    {
        var result = await _session
            .Query<AppUserFollows_FollowerCount.Result, AppUserFollows_FollowerCount>()
            .Where(r => r.UserId == userId)
            .FirstOrDefaultAsync();

        return result?.FollowerCount ?? 0;
    }

    public async Task<List<AppUserFollows.LeaderReference>> GetFollows(string followerId)
    {
        var follower = await GetOrCreateAppUserFollower(followerId);
        return follower.Follows;
    }

    private async Task<AppUserFollows> GetOrCreateAppUserFollower(string userId)
    {
        var appUserFollowsId = AppUserFollows.BuildId(userId);
        var appUserFollows = await _session.LoadAsync<AppUserFollows>(appUserFollowsId) ??
                             new AppUserFollows
                             {
                                 AppUserId = userId
                             };
        await _session.StoreAsync(appUserFollows);
        return appUserFollows;
    }
}