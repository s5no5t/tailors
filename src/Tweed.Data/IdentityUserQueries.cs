using NodaTime;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Tweed.Data.Entities;

namespace Tweed.Data;

public interface IIdentityUserQueries
{

    Task<List<TweedIdentityUser>> Search(string term);

}

public class IdentityUserQueries : IIdentityUserQueries
{
    private readonly IAsyncDocumentSession _session;

    public IdentityUserQueries(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task<List<TweedIdentityUser>> Search(string term)
    {
        return await _session.Query<TweedIdentityUser, IdentityUsers_ByUserName>()
            .Search(u => u.UserName, $"{term}*")
            .Take(20).ToListAsync();
    }
}
