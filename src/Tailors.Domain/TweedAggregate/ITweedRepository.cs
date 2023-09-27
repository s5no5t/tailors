namespace Tailors.Domain.TweedAggregate;

public interface ITweedRepository
{
    Task<TailorsTweed?> GetById(string id);
    Task<Dictionary<string, TailorsTweed>> GetByIds(IEnumerable<string> ids);
    Task<List<TailorsTweed>> GetAllByAuthorId(string authorId, int count);
    Task Create(TailorsTweed tweed);
    Task<List<TailorsTweed>> Search(string term);
    Task<List<TailorsTweed>> GetRecentTweeds(int count);
    Task<List<TailorsTweed>> GetFollowerTweeds(List<string> followedUserIds, int count);
}