using NodaTime;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Tweed.Data.Entities;
using Tweed.Data.Indexes;

namespace Tweed.Data;

public interface IAppUserQueries
{
    Task<List<AppUser>> Search(string term);
    Task AddLike(string tweedId, string userId, ZonedDateTime likedAt);
    Task RemoveLike(string tweedId, string userId);
}

public class AppUserQueries : IAppUserQueries
{
    private readonly IAsyncDocumentSession _session;

    public AppUserQueries(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task<List<AppUser>> Search(string term)
    {
        return await _session.Query<AppUser, AppUsers_ByUserName>()
            .Search(u => u.UserName, $"{term}*")
            .Take(20).ToListAsync();
    }

    public async Task AddLike(string tweedId, string userId, ZonedDateTime likedAt)
    {
        var appUser = await _session.LoadAsync<AppUser>(userId);
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
        var appUser = await _session.LoadAsync<AppUser>(userId);
        appUser.Likes.RemoveAll(lb => lb.TweedId == tweedId);
        var tweed = await _session.LoadAsync<Entities.Tweed>(tweedId);
        _session.CountersFor(tweed).Increment(Entities.Tweed.LikesCounterName, -1);
    }
}
