using NodaTime;
using Raven.Client.Documents.Session;
using Tweed.Domain;
using Tweed.Domain.Model;

namespace Tweed.Infrastructure;



public class TweedLikesService : ITweedLikesService
{
    private readonly IAsyncDocumentSession _session;

    public TweedLikesService(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task AddLike(string tweedId, string userId, ZonedDateTime likedAt)
    {
        var appUserLikes = await GetOrBuildAppUserLikes(userId);
        await _session.StoreAsync(appUserLikes);

        if (appUserLikes.Likes.Any(l => l.TweedId == tweedId))
            return;
        appUserLikes.Likes.Add(new AppUserLikes.TweedLike
        {
            TweedId = tweedId,
            CreatedAt = likedAt
        });
        var tweed = await _session.LoadAsync<Domain.Model.Tweed>(tweedId);
        _session.CountersFor(tweed).Increment(Domain.Model.Tweed.LikesCounterName);
    }

    public async Task RemoveLike(string tweedId, string userId)
    {
        var appUserLikes = await GetOrBuildAppUserLikes(userId);
        appUserLikes.Likes.RemoveAll(lb => lb.TweedId == tweedId);
        var tweed = await _session.LoadAsync<Domain.Model.Tweed>(tweedId);
        _session.CountersFor(tweed).Increment(Domain.Model.Tweed.LikesCounterName, -1);
    }

    public async Task<long> GetLikesCount(string tweedId)
    {
        var likesCounter =
            await _session.CountersFor(tweedId).GetAsync(Domain.Model.Tweed.LikesCounterName);
        return likesCounter ?? 0L;
    }

    public async Task<bool> DoesUserLikeTweed(string tweedId, string userId)
    {
        var appUserLikes = await GetOrBuildAppUserLikes(userId);
        return appUserLikes.Likes.Any(lb => lb.TweedId == tweedId);
    }

    private async Task<AppUserLikes> GetOrBuildAppUserLikes(string userId)
    {
        var appUserLikesId = AppUserLikes.BuildId(userId);
        return await _session.LoadAsync<AppUserLikes>(appUserLikesId) ??
               new AppUserLikes
               {
                   AppUserId = userId
               };
    }
}
