using NodaTime;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Tweed.Domain.Indexes;
using Tweed.Domain.Model;

namespace Tweed.Domain;

public interface ITweedService
{
    Task<List<Model.Tweed>> GetTweedsForUser(string userId);
    Task<Model.Tweed?> GetTweedById(string id);
    Task<Model.Tweed> CreateRootTweed(string authorId, string text, ZonedDateTime createdAt);
    Task<Model.Tweed> CreateReplyTweed(string authorId, string text, ZonedDateTime createdAt,
        string parentTweedId);
}

public sealed class TweedService : ITweedService
{
    private readonly IAsyncDocumentSession _session;

    public TweedService(IAsyncDocumentSession session)
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

    public Task<Model.Tweed?> GetTweedById(string id)
    {
        return _session.LoadAsync<Model.Tweed>(id)!;
    }

    public async Task<Model.Tweed> CreateRootTweed(string authorId, string text,
        ZonedDateTime createdAt)
    {
        Model.Tweed tweed = new()
        {
            AuthorId = authorId,
            Text = text,
            CreatedAt = createdAt
        };
        await _session.StoreAsync(tweed);

        var thread = await CreateThread(tweed.Id!);
        tweed.ThreadId = thread.Id;
        return tweed;
    }

    public async Task<Model.Tweed> CreateReplyTweed(string authorId, string text,
        ZonedDateTime createdAt, string parentTweedId)
    {
        var threadId = (await _session.LoadAsync<Model.Tweed>(parentTweedId)).ThreadId;
        Model.Tweed tweed = new()
        {
            AuthorId = authorId,
            Text = text,
            ParentTweedId = parentTweedId,
            ThreadId = threadId,
            CreatedAt = createdAt
        };
        await _session.StoreAsync(tweed);
        return tweed;
    }

    private async Task<TweedThread> CreateThread(string tweedId)
    {
        TweedThread thread = new()
        {
            Root =
            {
                TweedId = tweedId
            }
        };
        await _session.StoreAsync(thread);
        return thread;
    }
}
