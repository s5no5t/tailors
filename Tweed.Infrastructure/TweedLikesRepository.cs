using Raven.Client.Documents.Session;
using Tweed.Domain;
using Tweed.Domain.Model;

namespace Tweed.Infrastructure;

public class TweedLikesRepository : ITweedLikesRepository
{
    private readonly IAsyncDocumentSession _session;

    public TweedLikesRepository(IAsyncDocumentSession session)
    {
        _session = session;
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

    public Task<AppUserLikes?> Get(string userId)
    {
        throw new NotImplementedException();
    }

    public Task Create(AppUserLikes appUserLikes)
    {
        throw new NotImplementedException();
    }

    public void IncreaseLikesCounter(string tweedId)
    {
        _session.CountersFor(tweedId).Increment(Domain.Model.Tweed.LikesCounterName);
    }

    public void DecreaseLikesCounter(string tweedId)
    {
        _session.CountersFor(tweedId).Increment(Domain.Model.Tweed.LikesCounterName, -1);
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
