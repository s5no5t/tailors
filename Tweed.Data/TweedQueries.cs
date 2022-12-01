using NodaTime;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Tweed.Data.Entities;

namespace Tweed.Data;

public interface ITweedQueries
{
    Task StoreTweed(string text, string authorId, ZonedDateTime createdAt);
    Task<IEnumerable<Entities.Tweed>> GetLatestTweeds();
    Task<Entities.Tweed?> GetById(string id);
    Task AddLike(string id, string userId, ZonedDateTime likedAt);
    Task<IEnumerable<Entities.Tweed>> GetTweedsForUser(string userId);
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

    public async Task AddLike(string id, string userId, ZonedDateTime likedAt)
    {
        var tweed = await _session.LoadAsync<Entities.Tweed>(id);
        if (tweed.LikedBy.Any(l => l.UserId == userId))
            return;
        tweed.LikedBy.Add(new LikedBy
        {
            UserId = userId,
            LikedAt = likedAt
        });
    }

    public async Task<IEnumerable<Entities.Tweed>> GetTweedsForUser(string userId)
    {
        return await _session.Query<Entities.Tweed>()
            .OrderByDescending(t => t.CreatedAt)
            .Take(20)
            .ToListAsync();

    }

    public async Task StoreTweed(string text, string authorId, ZonedDateTime createdAt)
    {
        var tweed = new Entities.Tweed
        {
            CreatedAt = createdAt,
            AuthorId = authorId,
            Text = text
        };
        await _session.StoreAsync(tweed);
    }
}
