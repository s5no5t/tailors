using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Tweed.Data.Indexes;
using Tweed.Data.Model;

namespace Tweed.Data.Domain;

public interface ISearchService
{
    Task<List<AppUser>> SearchAppUsers(string term);
}

public class SearchService : ISearchService
{
    private readonly IAsyncDocumentSession _session;

    public SearchService(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task<List<AppUser>> SearchAppUsers(string term)
    {
        return await _session.Query<AppUser, AppUsers_ByUserName>()
            .Search(u => u.UserName, $"{term}*")
            .Take(20).ToListAsync();
    }
}
