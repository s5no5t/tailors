using OneOf;
using OneOf.Types;
using Tailors.Domain.TweedAggregate;

namespace Tailors.Domain.Test.TweedAggregate;

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
        throw new NotImplementedException();
    }

    public Task Create(Tweed tweed)
    {
        tweed.Id ??= _random.Next().ToString();
        _tweeds.Add(tweed.Id, tweed);
        return Task.CompletedTask;
    }

    public Task<List<Tweed>> Search(string term)
    {
        throw new NotImplementedException();
    }

    public Task<List<Tweed>> GetRecentTweeds(int count)
    {
        throw new NotImplementedException();
    }

    public Task<List<Tweed>> GetFollowerTweeds(List<string> followedUserIds, int count)
    {
        throw new NotImplementedException();
    }
}
