using NodaTime;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Tweed.Data.Indexes;

namespace Tweed.Data;

public interface ITweedQueries
{
    Task<List<Model.Tweed>> GetTweedsForUser(string userId);
    Task<Model.Tweed?> GetById(string id);
    Task StoreTweed(string text, string authorId, ZonedDateTime createdAt);
    Task<long> GetLikesCount(string tweedId);
    Task<List<Model.Tweed>> Search(string term);
}

public sealed class TweedQueries : ITweedQueries
{
    private readonly IAsyncDocumentSession _session;

    public TweedQueries(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task<List<Model.Tweed>> GetTweedsForUser(string userId)
    {
        return await _session.Query<Model.Tweed, Tweeds_ByAuthorIdAndCreatedAt>()
            .Where(t => t.AuthorId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(20)
            .ToListAsync();
    }

    public Task<Model.Tweed?> GetById(string id)
    {
        return _session.LoadAsync<Model.Tweed>(id)!;
    }

    public async Task StoreTweed(string text, string authorId, ZonedDateTime createdAt)
    {
        var tweed = new Model.Tweed
        {
            CreatedAt = createdAt,
            AuthorId = authorId,
            Text = text
        };
        await _session.StoreAsync(tweed);
    }

    public async Task<long> GetLikesCount(string tweedId)
    {
        var likesCounter = await _session.CountersFor(tweedId).GetAsync(Model.Tweed.LikesCounterName);
        return likesCounter ?? 0L;
    }

    public async Task<List<Model.Tweed>> Search(string term)
    {
        return await _session.Query<Model.Tweed, Tweeds_ByText>()
            .Search(t => t.Text, term)
            .Take(20).ToListAsync();
    }
}
