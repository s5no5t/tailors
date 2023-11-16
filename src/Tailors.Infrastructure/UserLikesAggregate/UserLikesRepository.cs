using OneOf;
using OneOf.Types;
using Raven.Client.Documents.Session;
using Tailors.Domain.UserLikesAggregate;

namespace Tailors.Infrastructure.UserLikesAggregate;

public class UserLikesRepository(IAsyncDocumentSession session) : IUserLikesRepository
{
    private const string LikesCounterName = "Likes";

    public async Task<OneOf<UserLikes, None>> GetById(string userLikesId)
    {
        var likes = await session.LoadAsync<UserLikes>(userLikesId);
        return likes is null ? new None() : likes;
    }

    public async Task Create(UserLikes userLikes)
    {
        await session.StoreAsync(userLikes);
    }

    public async Task<long> GetLikesCounter(string tweedId)
    {
        var likesCounter =
            await session.CountersFor(tweedId).GetAsync(LikesCounterName);
        return likesCounter ?? 0L;
    }

    public void IncreaseLikesCounter(string tweedId)
    {
        session.CountersFor(tweedId).Increment(LikesCounterName);
    }

    public void DecreaseLikesCounter(string tweedId)
    {
        session.CountersFor(tweedId).Increment(LikesCounterName, -1);
    }
}
