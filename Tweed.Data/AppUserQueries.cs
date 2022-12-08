using NodaTime;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Session;
using Tweed.Data.Entities;

namespace Tweed.Data;

public class
    AppUsers_FollowerCount : AbstractIndexCreationTask<AppUser, AppUsers_FollowerCount.Result>
{
    public AppUsers_FollowerCount()
    {
        Map = appUsers => from appUser in appUsers
            select new
            {
                AppUserId = appUser.Id,
                FollowerCount = 1
            };

        Reduce = results => from result in results
            group result by result.AppUserId
            into g
            select new
            {
                AppUserId = g.Key,
                FollowerCount = g.Sum(x => x.FollowerCount)
            };
    }

    public class Result
    {
        public string AppUserId { get; set; }
        public int FollowerCount { get; set; }
    }
}

public interface IAppUserQueries
{
    Task AddFollower(string leaderId, string followerId, ZonedDateTime createdAt);
    Task RemoveFollower(string leaderId, string userId);
    Task<int> GetFollowerCount(string userId);
}

public class AppUserQueries : IAppUserQueries
{
    private readonly IAsyncDocumentSession _session;

    public AppUserQueries(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task AddFollower(string leaderId, string followerId, ZonedDateTime createdAt)
    {
        var follower = await _session.LoadAsync<AppUser>(followerId);
        if (follower.Follows.Any(f => f.LeaderId == leaderId))
            return;

        follower.Follows.Add(new Follows
        {
            LeaderId = leaderId,
            CreatedAt = createdAt
        });
    }

    public async Task RemoveFollower(string leaderId, string userId)
    {
        var follower = await _session.LoadAsync<AppUser>(userId);
        follower.Follows.RemoveAll(f => f.LeaderId == leaderId);
    }

    public async Task<int> GetFollowerCount(string userId)
    {
        var result = await _session.Query<AppUsers_FollowerCount.Result, AppUsers_FollowerCount>()
            .Where(r => r.AppUserId == userId)
            .FirstOrDefaultAsync();

        return result?.FollowerCount ?? 0;
    }
}
