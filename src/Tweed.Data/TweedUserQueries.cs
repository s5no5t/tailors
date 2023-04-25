using NodaTime;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Tweed.Data.Entities;

namespace Tweed.Data;

public interface ITweedUserQueries
{
    Task AddFollower(string leaderId, string followerId, ZonedDateTime createdAt);
    Task RemoveFollower(string leaderId, string userId);
    Task<int> GetFollowerCount(string userId);
    Task AddLike(string tweedId, string userId, ZonedDateTime likedAt);
    Task RemoveLike(string tweedId, string userId);
    Task<TweedUser> FindByIdentityUserId(string identityUserId);
}

public class TweedUserQueries : ITweedUserQueries
{
    private readonly IAsyncDocumentSession _session;

    public TweedUserQueries(IAsyncDocumentSession session)
    {
        _session = session;
    }
    
    public async Task AddFollower(string leaderId, string followerId, ZonedDateTime createdAt)
    {
        var follower = await _session.LoadAsync<TweedUser>(followerId);
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
        var follower = await _session.LoadAsync<TweedUser>(userId);
        follower.Follows.RemoveAll(f => f.LeaderId == leaderId);
    }

    public async Task<int> GetFollowerCount(string userId)
    {
        var result = await _session.Query<TweedUsers_FollowerCount.Result, TweedUsers_FollowerCount>()
            .Where(r => r.UserId == userId)
            .FirstOrDefaultAsync();

        return result?.FollowerCount ?? 0;
    }
    
    public async Task AddLike(string tweedId, string userId, ZonedDateTime likedAt)
    {
        var appUser = await _session.LoadAsync<TweedUser>(userId);
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
        var appUser = await _session.LoadAsync<TweedUser>(userId);
        appUser.Likes.RemoveAll(lb => lb.TweedId == tweedId);
        var tweed = await _session.LoadAsync<Entities.Tweed>(tweedId);
        _session.CountersFor(tweed).Increment(Entities.Tweed.LikesCounterName, -1);
    }

    public Task<TweedUser> FindByIdentityUserId(string identityUserId)
    {
        throw new NotImplementedException();
    }
}
