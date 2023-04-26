using NodaTime;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Tweed.Data.Entities;
using Tweed.Data.Indexes;

namespace Tweed.Data;

public interface IAppUserFollowsQueries
{
    Task AddFollower(string leaderId, string followerId, ZonedDateTime createdAt);
    Task RemoveFollower(string leaderId, string followerId);
    Task<int> GetFollowerCount(string userId);
    Task<List<Follows>> GetFollows(string userId);
}

public class AppUserFollowsQueries : IAppUserFollowsQueries
{
    private readonly IAsyncDocumentSession _session;

    public AppUserFollowsQueries(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task AddFollower(string leaderId, string followerId, ZonedDateTime createdAt)
    {
        var appUserFollows = await GetOrCreateAppUserFollower(followerId);
        
        if (appUserFollows.Follows.Any(f => f.LeaderId == leaderId))
            return;

        appUserFollows.Follows.Add(new Follows
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
        var result = await _session.Query<AppUserFollows_FollowerCount.Result, AppUserFollows_FollowerCount>()
            .Where(r => r.UserId == userId)
            .FirstOrDefaultAsync();

        return result?.FollowerCount ?? 0;
    }

    public async Task<List<Follows>> GetFollows(string followerId)
    {
        var follower = await GetOrCreateAppUserFollower(followerId);
        return follower.Follows;
    }

    private async Task<AppUserFollows> GetOrCreateAppUserFollower(string userId)
    {
        var appUserFollowsId = AppUserFollows.BuildId(userId);
        var appUserFollows = await _session.LoadAsync<AppUserFollows>(appUserFollowsId) ?? new AppUserFollows
        {
            AppUserId = userId
        };
        await _session.StoreAsync(appUserFollows);
        return appUserFollows;
    }
}
