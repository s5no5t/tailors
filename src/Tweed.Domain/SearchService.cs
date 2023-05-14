namespace Tweed.Domain;

public interface ISearchService
{
    Task<List<Model.Tweed>> SearchTweeds(string term);
}