using NodaTime;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Tweed.Data.Entities;

namespace Tweed.Data;

public interface IIdentityUserQueries
{
    Task AddFollower(string leaderId, string followerId, ZonedDateTime createdAt);
    Task RemoveFollower(string leaderId, string userId);
    Task<int> GetFollowerCount(string userId);
    Task<List<TweedIdentityUser>> Search(string term);
    Task AddLike(string tweedId, string userId, ZonedDateTime likedAt);
    Task RemoveLike(string tweedId, string userId);
}

public class IdentityUserQueries : IIdentityUserQueries
{
    private readonly IAsyncDocumentSession _session;

    public IdentityUserQueries(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task AddFollower(string leaderId, string followerId, ZonedDateTime createdAt)
    {
        var follower = await _session.LoadAsync<TweedIdentityUser>(followerId);
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
        var follower = await _session.LoadAsync<TweedIdentityUser>(userId);
        follower.Follows.RemoveAll(f => f.LeaderId == leaderId);
    }

    public async Task<int> GetFollowerCount(string userId)
    {
        var result = await _session.Query<IdentityUsers_FollowerCount.Result, IdentityUsers_FollowerCount>()
            .Where(r => r.UserId == userId)
            .FirstOrDefaultAsync();

        return result?.FollowerCount ?? 0;
    }

    public async Task<List<TweedIdentityUser>> Search(string term)
    {
        return await _session.Query<TweedIdentityUser, IdentityUsers_ByUserName>()
            .Search(u => u.UserName, $"{term}*")
            .Take(20).ToListAsync();
    }

    public async Task AddLike(string tweedId, string userId, ZonedDateTime likedAt)
    {
        var appUser = await _session.LoadAsync<TweedIdentityUser>(userId);
        if (appUser.Likes.Any(l => l.TweedId == tweedId))
            return;
        appUser.Likes.Add(new TweedLike
        {
            TweedId = tweedId,
            CreatedAt = likedAt
        });
        var tweed = await _session.LoadAsync<Entities.Tweed>(tweedId);
        _session.CountersFor(tweed).Increment(Entities.Tweed.LikesCounterName);
    }

    public async Task RemoveLike(string tweedId, string userId)
    {
        var appUser = await _session.LoadAsync<TweedIdentityUser>(userId);
        appUser.Likes.RemoveAll(lb => lb.TweedId == tweedId);
        var tweed = await _session.LoadAsync<Entities.Tweed>(tweedId);
        _session.CountersFor(tweed).Increment(Entities.Tweed.LikesCounterName, -1);
    }
}
