using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Tweed.User.Domain;
using Tweed.User.Infrastructure.Indexes;

namespace Tweed.User.Infrastructure;

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
            .Query<UserFollows_FollowerCount.Result, UserFollows_FollowerCount>()
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
