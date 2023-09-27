using Raven.Client.Documents.Session;
using Tailors.Domain.UserLikes;

namespace Tailors.Infrastructure.UserLikes;

public class UserLikesRepository : IUserLikesRepository
{
    private const string LikesCounterName = "Likes";
    private readonly IAsyncDocumentSession _session;

    public UserLikesRepository(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task<Domain.UserLikes.UserLikes?> GetById(string userLikesId)
    {
        return await _session.LoadAsync<Domain.UserLikes.UserLikes>(userLikesId);
    }

    public async Task Create(Domain.UserLikes.UserLikes userLikes)
    {
        await _session.StoreAsync(userLikes);
    }

    public async Task<long> GetLikesCounter(string tweedId)
    {
        var likesCounter =
            await _session.CountersFor(tweedId).GetAsync(LikesCounterName);
        return likesCounter ?? 0L;
    }

    public void IncreaseLikesCounter(string tweedId)
    {
        _session.CountersFor(tweedId).Increment(LikesCounterName);
    }

    public void DecreaseLikesCounter(string tweedId)
    {
        _session.CountersFor(tweedId).Increment(LikesCounterName, -1);
    }
}