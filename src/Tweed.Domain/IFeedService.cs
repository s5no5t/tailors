namespace Tweed.Domain;

public interface IFeedService
{
    Task<List<Domain.Model.Tweed>> GetFeed(string appUserId, int page);
}