using OneOf;
using OneOf.Types;
using Raven.Client.Documents.Session;
using Tailors.Domain.UserLikesAggregate;

namespace Tailors.Infrastructure.UserLikesAggregate;

public class UserLikesRepository : IUserLikesRepository
{
    private const string LikesCounterName = "Likes";
    private readonly IAsyncDocumentSession _session;

    public UserLikesRepository(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task<OneOf<UserLikes, None>> GetById(string userLikesId)
    {
        var likes = await _session.LoadAsync<UserLikes>(userLikesId);
        return likes is null ? new None() : likes;
    }

    public async Task Create(UserLikes userLikes)
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