namespace Tailors.Tweed.Domain.TweedAggregate;

public interface ITweedRepository
{
    Task<Tweed?> GetById(string id);
    Task<Dictionary<string, Tweed>> GetByIds(IEnumerable<string> ids);
    Task<List<Tweed>> GetAllByAuthorId(string authorId, int count);
    Task Create(Tweed tweed);
    Task<List<Tweed>> Search(string term);
    Task<List<Tweed>> GetRecentTweeds(int count);
    Task<List<Tweed>> GetFollowerTweeds(List<string> followedUserIds, int count);
}