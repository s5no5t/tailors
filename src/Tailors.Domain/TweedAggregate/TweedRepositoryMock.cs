using OneOf;
using OneOf.Types;

namespace Tailors.Domain.TweedAggregate;

public class TweedRepositoryMock : ITweedRepository
{
    private readonly Random _random = new();
    private readonly Dictionary<string, Tweed> _tweeds = new();

    public Task<OneOf<Tweed, None>> GetById(string id)
    {
        _tweeds.TryGetValue(id, out var tweed);

        if (tweed is not null)
            return Task.FromResult<OneOf<Tweed, None>>(tweed);

        return Task.FromResult<OneOf<Tweed, None>>(new None());
    }

    public Task<Dictionary<string, Tweed>> GetByIds(IEnumerable<string> ids)
    {
        var tweeds = _tweeds.Where(t => ids.Contains(t.Key)).ToDictionary(t => t.Key, t => t.Value);
        return Task.FromResult(tweeds);
    }

    public Task<List<Tweed>> GetAllByAuthorId(string authorId, int count)
    {
        var tweeds = _tweeds
            .Where(t => t.Value.AuthorId == authorId)
            .Take(count)
            .Select(t => t.Value).ToList();
        return Task.FromResult(tweeds);
    }

    public Task Create(Tweed tweed)
    {
        tweed.Id ??= _random.Next().ToString();
        _tweeds.Add(tweed.Id, tweed);
        return Task.CompletedTask;
    }

    public Task<List<Tweed>> Search(string term)
    {
        var tweeds = _tweeds.Where(t => t.Value.Text.Contains(term)).Select(t => t.Value).ToList();
        return Task.FromResult(tweeds);
    }

    public Task<List<Tweed>> GetRecentTweeds(int count)
    {
        var tweeds = _tweeds
            .OrderByDescending(t => t.Value.CreatedAt)
            .Take(count)
            .Select(t => t.Value).ToList();
        return Task.FromResult(tweeds);
    }

    public Task<List<Tweed>> GetFollowerTweeds(List<string> followedUserIds, int count)
    {
        var tweeds = _tweeds
            .Where(t => followedUserIds.Contains(t.Value.AuthorId))
            .Take(count)
            .Select(t => t.Value).ToList();
        return Task.FromResult(tweeds);
    }

    public Task<List<Tweed>> GetUpTo20ReplyTweeds(IReadOnlyCollection<string> leadingTweedIds)
    {
        var tweeds = _tweeds
            .Where(t => t.Value.LeadingTweedIds.HasPrefix(leadingTweedIds))
            .Select(t => t.Value).ToList();
        return Task.FromResult(tweeds);
    }
}

internal static class CollectionExtensions
{
    internal static bool HasPrefix(this IReadOnlyCollection<string> leadingTweedIds, IReadOnlyCollection<string> prefix)
    {
        return leadingTweedIds.Count >= prefix.Count
               && leadingTweedIds.Take(prefix.Count).SequenceEqual(prefix);
    }
}
