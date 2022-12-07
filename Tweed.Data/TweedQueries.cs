using NodaTime;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Tweed.Data.Entities;

namespace Tweed.Data;

public class Tweeds_ByAuthorId : AbstractIndexCreationTask<Entities.Tweed>
{
    public Tweeds_ByAuthorId()
    {
        Map = tweeds => from tweed in tweeds
            select new
            {
                tweed.AuthorId,
                tweed.CreatedAt
            };
    }
}

public interface ITweedQueries
{
    Task StoreTweed(string text, string authorId, ZonedDateTime createdAt);
    Task<List<Entities.Tweed>> GetFeed(string userId);
    Task<Entities.Tweed?> GetById(string id);
    Task AddLike(string id, string userId, ZonedDateTime likedAt);
    Task<List<Entities.Tweed>> GetTweedsForUser(string userId);
    Task RemoveLike(string id, string userId);
}

public sealed class TweedQueries : ITweedQueries
{
    private readonly IAsyncDocumentSession _session;

    public TweedQueries(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task<List<Entities.Tweed>> GetFeed(string userId)
    {
        return await _session.Query<Entities.Tweed, Tweeds_ByAuthorId>()
            .Where(t => t.AuthorId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(20)
            .ToListAsync();
    }

    public Task<Entities.Tweed?> GetById(string id)
    {
        return _session.LoadAsync<Entities.Tweed>(id)!;
    }

    public async Task AddLike(string id, string userId, ZonedDateTime likedAt)
    {
        var tweed = await _session.LoadAsync<Entities.Tweed>(id);
        if (tweed.Likes.Any(l => l.UserId == userId))
            return;
        tweed.Likes.Add(new Like
        {
            UserId = userId,
            CreatedAt = likedAt
        });
    }

    public async Task RemoveLike(string id, string userId)
    {
        var tweed = await _session.LoadAsync<Entities.Tweed>(id);
        tweed.Likes.RemoveAll(lb => lb.UserId == userId);
    }

    public async Task<List<Entities.Tweed>> GetTweedsForUser(string userId)
    {
        return await _session.Query<Entities.Tweed>()
            .Where(t => t.AuthorId == userId)
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

