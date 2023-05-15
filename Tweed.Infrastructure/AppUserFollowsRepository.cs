using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Tweed.Domain;
using Tweed.Domain.Model;
using Tweed.Infrastructure.Indexes;

namespace Tweed.Infrastructure;

public class AppUserFollowsRepository : IAppUserFollowsRepository
{
    private readonly IAsyncDocumentSession _session;

    public AppUserFollowsRepository(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task<int> GetFollowerCount(string userId)
    {
        var result = await _session
            .Query<AppUserFollows_FollowerCount.Result, AppUserFollows_FollowerCount>()
            .Where(r => r.UserId == userId)
            .FirstOrDefaultAsync();

        return result?.FollowerCount ?? 0;
    }

    public async Task<AppUserFollows?> GetById(string appUserFollowsId)
    {
        return await _session.LoadAsync<AppUserFollows>(appUserFollowsId);
    }

    public async Task Create(AppUserFollows appUserFollows)
    {
        await _session.StoreAsync(appUserFollows);
    }
}
