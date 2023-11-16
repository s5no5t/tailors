using OneOf;
using OneOf.Types;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Tailors.Domain.UserFollowsAggregate;
using Tailors.Infrastructure.UserFollowsAggregate.Indexes;

namespace Tailors.Infrastructure.UserFollowsAggregate;

public class UserFollowsRepository(IAsyncDocumentSession session) : IUserFollowsRepository
{
    public async Task<int> GetFollowerCount(string userId)
    {
        var result = await session
            .Query<UserFollowsFollowerCount.Result, UserFollowsFollowerCount>()
            .Where(r => r.UserId == userId)
            .FirstOrDefaultAsync();

        return result?.FollowerCount ?? 0;
    }

    public async Task<OneOf<UserFollows, None>> GetById(string userFollowsId)
    {
        var userFollows = await session.LoadAsync<UserFollows>(userFollowsId);
        return userFollows is null ? new None() : userFollows;
    }

    public async Task Create(UserFollows userFollows)
    {
        await session.StoreAsync(userFollows);
    }
}
