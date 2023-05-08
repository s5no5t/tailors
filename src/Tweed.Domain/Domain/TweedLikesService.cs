using NodaTime;
using Raven.Client.Documents.Session;
using Tweed.Domain.Model;

namespace Tweed.Domain.Domain;

public interface ITweedLikesService
{
    Task<List<AppUserLikes.TweedLike>> GetLikes(string userId);
    Task AddLike(string tweedId, string userId, ZonedDateTime likedAt);
    Task RemoveLike(string tweedId, string userId);
    Task<long> GetLikesCount(string tweedId);
}

public class TweedLikesService : ITweedLikesService
{
    private readonly IAsyncDocumentSession _session;

    public TweedLikesService(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task<List<AppUserLikes.TweedLike>> GetLikes(string userId)
    {
        var appUserLikes = await GetOrCreateAppUserLikes(userId);
        return appUserLikes.Likes;
    }

    public async Task AddLike(string tweedId, string userId, ZonedDateTime likedAt)
    {
        var appUserLikes = await GetOrCreateAppUserLikes(userId);
        if (appUserLikes.Likes.Any(l => l.TweedId == tweedId))
            return;
        appUserLikes.Likes.Add(new AppUserLikes.TweedLike
        {
            TweedId = tweedId,
            CreatedAt = likedAt
        });
        var tweed = await _session.LoadAsync<Model.Tweed>(tweedId);
        _session.CountersFor(tweed).Increment(Model.Tweed.LikesCounterName);
    }

    public async Task RemoveLike(string tweedId, string userId)
    {
        var appUserLikes = await GetOrCreateAppUserLikes(userId);
        appUserLikes.Likes.RemoveAll(lb => lb.TweedId == tweedId);
        var tweed = await _session.LoadAsync<Model.Tweed>(tweedId);
        _session.CountersFor(tweed).Increment(Model.Tweed.LikesCounterName, -1);
    }

    public async Task<long> GetLikesCount(string tweedId)
    {
        var likesCounter =
            await _session.CountersFor(tweedId).GetAsync(Model.Tweed.LikesCounterName);
        return likesCounter ?? 0L;
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
