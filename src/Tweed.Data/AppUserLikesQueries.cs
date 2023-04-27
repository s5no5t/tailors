using NodaTime;
using Raven.Client.Documents.Session;
using Tweed.Data.Entities;

namespace Tweed.Data;

public interface IAppUserLikesQueries
{
    Task<List<TweedLike>> GetLikes(string userId);
    Task AddLike(string tweedId, string userId, ZonedDateTime likedAt);
    Task RemoveLike(string tweedId, string userId);
}

public class AppUserLikesQueries : IAppUserLikesQueries
{
    private readonly IAsyncDocumentSession _session;

    public AppUserLikesQueries(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task<List<TweedLike>> GetLikes(string userId)
    {
        var appUserLikes = await GetOrCreateAppUserLikes(userId);
        return appUserLikes.Likes;
    }

    public async Task AddLike(string tweedId, string userId, ZonedDateTime likedAt)
    {
        var appUserLikes = await GetOrCreateAppUserLikes(userId);
        if (appUserLikes.Likes.Any(l => l.TweedId == tweedId))
            return;
        appUserLikes.Likes.Add(new TweedLike
        {
            TweedId = tweedId,
            CreatedAt = likedAt
        });
        var tweed = await _session.LoadAsync<Entities.Tweed>(tweedId);
        _session.CountersFor(tweed).Increment(Entities.Tweed.LikesCounterName);
    }

    public async Task RemoveLike(string tweedId, string userId)
    {
        var appUserLikes = await GetOrCreateAppUserLikes(userId);
        appUserLikes.Likes.RemoveAll(lb => lb.TweedId == tweedId);
        var tweed = await _session.LoadAsync<Entities.Tweed>(tweedId);
        _session.CountersFor(tweed).Increment(Entities.Tweed.LikesCounterName, -1);
    }

    private async Task<AppUserLikes> GetOrCreateAppUserLikes(string userId)
    {
        var appUserLikesId = AppUserLikes.BuildId(userId);
        var appUserLikes = await _session.LoadAsync<AppUserLikes>(appUserLikesId) ??
                           new AppUserLikes
                           {
                               AppUserId = userId
                           };
        await _session.StoreAsync(appUserLikes);
        return appUserLikes;
    }
}
