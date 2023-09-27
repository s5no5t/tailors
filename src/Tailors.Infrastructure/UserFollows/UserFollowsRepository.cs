using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Tailors.Domain.UserFollows;
using Tailors.Infrastructure.UserFollows.Indexes;

namespace Tailors.Infrastructure.UserFollows;

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

    public async Task<Domain.UserFollows.UserFollows?> GetById(string userFollowsId)
    {
        return await _session.LoadAsync<Domain.UserFollows.UserFollows>(userFollowsId);
    }

    public async Task Create(Domain.UserFollows.UserFollows userFollows)
    {
        await _session.StoreAsync(userFollows);
    }
}
