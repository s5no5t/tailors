using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Tweed.Data.Entities;

namespace Tweed.Data;

public interface ITweedQueries
{
    Task StoreTweed(Entities.Tweed tweed);
    Task<IEnumerable<Entities.Tweed>> GetLatestTweeds();
    Task<Entities.Tweed?> GetById(string id);
    Task AddLike(string id, string userId);
}

public sealed class TweedQueries : ITweedQueries
{
    private readonly IAsyncDocumentSession _session;

    public TweedQueries(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task<IEnumerable<Entities.Tweed>> GetLatestTweeds()
    {
        return await _session.Query<Entities.Tweed>().OrderByDescending(t => t.CreatedAt).Take(20)
            .ToListAsync();
    }

    public Task<Entities.Tweed?> GetById(string id)
    {
        return _session.LoadAsync<Entities.Tweed>(id)!;
    }

    public async Task AddLike(string id, string userId)
    {
        var tweed = await _session.LoadAsync<Entities.Tweed>(id);
        if (tweed.LikedBy.Any(l => l.UserId == userId))
            return;
        tweed.LikedBy.Add(new LikedBy
        {
            UserId = userId
        });
    }

    public async Task StoreTweed(Entities.Tweed tweed)
    {
        if (tweed.CreatedAt is null)
            throw new ArgumentException("tweed.CreatedAt must not be null");
        if (tweed.AuthorId is null)
            throw new ArgumentException("tweed.AuthorId must not be null");

        await _session.StoreAsync(tweed);
    }
}
