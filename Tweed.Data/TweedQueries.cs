using NodaTime;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Tweed.Data.Entities;

namespace Tweed.Data;

public interface ITweedQueries
{
    Task<List<Entities.Tweed>> GetFeed(string userId);
    Task<List<Entities.Tweed>> GetTweedsForUser(string userId);
    Task<Entities.Tweed?> GetById(string id);
    Task StoreTweed(string text, string authorId, ZonedDateTime createdAt);
    Task<long> GetLikesCount(string tweedId);
    Task<List<Entities.Tweed>> Search(string term);
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
        var user = await _session.LoadAsync<AppUser>(userId);
        var followedUserIds = user.Follows.Select(f => f.LeaderId).ToList();

        followedUserIds.Add(userId); // show her own Tweeds as well

        return await _session.Query<Entities.Tweed, Tweeds_ByAuthorIdAndCreatedAt>()
            .Where(t => t.AuthorId.In(followedUserIds))
            .OrderByDescending(t => t.CreatedAt)
            .Take(20)
            .ToListAsync();
    }

    public async Task<List<Entities.Tweed>> GetTweedsForUser(string userId)
    {
        return await _session.Query<Entities.Tweed, Tweeds_ByAuthorIdAndCreatedAt>()
            .Where(t => t.AuthorId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(20)
            .ToListAsync();
    }

    public Task<Entities.Tweed?> GetById(string id)
    {
        return _session.LoadAsync<Entities.Tweed>(id)!;
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

    public async Task<long> GetLikesCount(string tweedId)
    {
        var likesCounter = await _session.CountersFor(tweedId).GetAsync(Entities.Tweed.LikesCounterName);
        return likesCounter ?? 0L;
    }

    public async Task<List<Entities.Tweed>> Search(string term)
    {
        return await _session.Query<Entities.Tweed, Tweeds_ByText>()
            .Search(t => t.Text, term)
            .Take(20).ToListAsync();
    }
}
