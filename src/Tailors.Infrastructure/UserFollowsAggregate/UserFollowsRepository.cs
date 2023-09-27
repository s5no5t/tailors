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

    public async Task<Domain.UserFollowsAggregate.UserFollows?> GetById(string userFollowsId)
    {
        return await _session.LoadAsync<Domain.UserFollowsAggregate.UserFollows>(userFollowsId);
    }

    public async Task Create(Domain.UserFollowsAggregate.UserFollows userFollows)
    {
        await _session.StoreAsync(userFollows);
    }
}
