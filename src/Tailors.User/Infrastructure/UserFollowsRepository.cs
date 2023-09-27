using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Tailors.User.Domain.UserFollowsAggregate;
using Tailors.User.Infrastructure.Indexes;

namespace Tailors.User.Infrastructure;

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

    public async Task<UserFollows?> GetById(string userFollowsId)
    {
        return await _session.LoadAsync<UserFollows>(userFollowsId);
    }

    public async Task Create(UserFollows userFollows)
    {
        await _session.StoreAsync(userFollows);
    }
}
