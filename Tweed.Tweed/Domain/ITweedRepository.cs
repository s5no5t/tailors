using Tweed.Domain.Model;

namespace Tweed.Domain;

public interface ITweedRepository
{
    Task<TheTweed?> GetById(string id);
    Task<Dictionary<string, TheTweed>> GetByIds(IEnumerable<string> ids);
    Task<List<TheTweed>> GetAllByAuthorId(string authorId, int count);
    Task Create(TheTweed theTweed);
    Task<List<TheTweed>> Search(string term);
    Task<List<TheTweed>> GetRecentTweeds(int count);
    Task<List<TheTweed>> GetFollowerTweeds(List<string> followedUserIds, int count);
}