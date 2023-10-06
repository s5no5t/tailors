using JetBrains.Annotations;
using OneOf;
using OneOf.Types;

namespace Tailors.Domain.TweedAggregate;

public interface ITweedRepository
{
    [MustUseReturnValue]
    Task<OneOf<Tweed, None>> GetById(string id);

    [MustUseReturnValue]
    Task<Dictionary<string, Tweed>> GetByIds(IEnumerable<string> ids);

    [MustUseReturnValue]
    Task<List<Tweed>> GetAllByAuthorId(string authorId, int count);

    Task Create(Tweed tweed);

    [MustUseReturnValue]
    Task<List<Tweed>> Search(string term);

    [MustUseReturnValue]
    Task<List<Tweed>> GetRecentTweeds(int count);

    [MustUseReturnValue]
    Task<List<Tweed>> GetFollowerTweeds(List<string> followedUserIds, int count);
}
