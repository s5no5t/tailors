using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Tweed.Domain.Indexes;
using Tweed.Domain.Model;

namespace Tweed.Domain.Domain;

public interface ISearchService
{
    Task<List<AppUser>> SearchAppUsers(string term);
    Task<List<Model.Tweed>> SearchTweeds(string term);
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
    
    public async Task<List<Model.Tweed>> SearchTweeds(string term)
    {
        return await _session.Query<Model.Tweed, Tweeds_ByText>()
            .Search(t => t.Text, term)
            .Take(20).ToListAsync();
    }
}
