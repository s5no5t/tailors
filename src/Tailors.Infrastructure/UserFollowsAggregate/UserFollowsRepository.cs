using OneOf;
using OneOf.Types;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Tailors.Domain.UserFollowsAggregate;
using Tailors.Infrastructure.UserFollowsAggregate.Indexes;

namespace Tailors.Infrastructure.UserFollowsAggregate;

public class UserFollowsRepository : IUserFollowsRepository
{
    private readonly IAsyncDocumentSession _session;

    public UserFollowsRepository(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task<int> GetFollowerCount(string userId)
    {
        var result = await _session
            .Query<UserFollowsFollowerCount.Result, UserFollowsFollowerCount>()
            .Where(r => r.UserId == userId)
            .FirstOrDefaultAsync();

        return result?.FollowerCount ?? 0;
    }

    public async Task<OneOf<UserFollows, None>> GetById(string userFollowsId)
    {
        var userFollows = await _session.LoadAsync<UserFollows>(userFollowsId);
        return userFollows is null ? new None() : userFollows;
    }

    public async Task Create(UserFollows userFollows)
    {
        await _session.StoreAsync(userFollows);
    }
}
