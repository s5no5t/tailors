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

    public async Task<AppUserLikes?> Get(string appUserLikesId)
    {
        return await _session.LoadAsync<AppUserLikes>(appUserLikesId);
    }

    public async Task Create(AppUserLikes appUserLikes)
    {
        await _session.StoreAsync(appUserLikes);
    }

    public async Task<long> GetLikesCounter(string tweedId)
    {
        var likesCounter =
            await _session.CountersFor(tweedId).GetAsync(Domain.Model.Tweed.LikesCounterName);
        return likesCounter ?? 0L;
    }

    public void IncreaseLikesCounter(string tweedId)
    {
        _session.CountersFor(tweedId).Increment(Domain.Model.Tweed.LikesCounterName);
    }

    public void DecreaseLikesCounter(string tweedId)
    {
        _session.CountersFor(tweedId).Increment(Domain.Model.Tweed.LikesCounterName, -1);
    }
}
