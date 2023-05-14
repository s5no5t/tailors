using NodaTime;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Tweed.Domain;
using Tweed.Domain.Model;
using Tweed.Infrastructure.Indexes;

namespace Tweed.Infrastructure;

public sealed class TweedRepository : ITweedRepository
{
    private readonly IAsyncDocumentSession _session;

    public TweedRepository(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task<List<Domain.Model.Tweed>> GetTweedsForUser(string userId)
    {
        return await _session.Query<Domain.Model.Tweed, Tweeds_ByAuthorIdAndCreatedAt>()
            .Where(t => t.AuthorId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(20)
            .ToListAsync();
    }

    public Task<Domain.Model.Tweed?> GetTweedById(string id)
    {
        return _session.LoadAsync<Domain.Model.Tweed>(id)!;
    }

    public async Task<Domain.Model.Tweed> CreateRootTweed(string authorId, string text,
        ZonedDateTime createdAt)
    {
        Domain.Model.Tweed tweed = new()
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

    public async Task<Domain.Model.Tweed> CreateReplyTweed(string authorId, string text,
        ZonedDateTime createdAt, string parentTweedId)
    {
        var threadId = (await _session.LoadAsync<Domain.Model.Tweed>(parentTweedId)).ThreadId;
        Domain.Model.Tweed tweed = new()
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

    public async Task<List<Domain.Model.Tweed>> SearchTweeds(string term)
    {
        return await _session.Query<Domain.Model.Tweed, Tweeds_ByText>()
            .Search(t => t.Text, term)
            .Take(20).ToListAsync();
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