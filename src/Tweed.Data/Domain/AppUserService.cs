using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Tweed.Data.Indexes;
using Tweed.Data.Model;

namespace Tweed.Data.Domain;

public interface IAppUserService
{
    Task<List<AppUser>> Search(string term);
}

public class AppUserService : IAppUserService
{
    private readonly IAsyncDocumentSession _session;

    public AppUserService(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task<List<AppUser>> Search(string term)
    {
        return await _session.Query<AppUser, AppUsers_ByUserName>()
            .Search(u => u.UserName, $"{term}*")
            .Take(20).ToListAsync();
    }
}
