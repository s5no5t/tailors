using Tweed.Domain.Model;

namespace Tweed.Domain;

public interface ISearchService
{
    Task<List<AppUser>> SearchAppUsers(string term);
    Task<List<Domain.Model.Tweed>> SearchTweeds(string term);
}