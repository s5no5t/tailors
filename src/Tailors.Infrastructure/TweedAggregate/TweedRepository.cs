using OneOf;
using OneOf.Types;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Tailors.Domain.TweedAggregate;
using Tailors.Infrastructure.TweedAggregate.Indexes;

namespace Tailors.Infrastructure.TweedAggregate;

public sealed class TweedRepository(IAsyncDocumentSession session) : ITweedRepository
{
    public async Task<OneOf<Tweed, None>> GetById(string id)
    {
        var tweed = await session.LoadAsync<Tweed>(id)!;
        return tweed is null ? new None() : tweed;
    }

    public Task<Dictionary<string, Tweed>> GetByIds(IEnumerable<string> ids)
    {
        return session.LoadAsync<Tweed>(ids);
    }

    public async Task Create(Tweed tweed)
    {
        await session.StoreAsync(tweed);
    }

    public async Task<List<Tweed>> GetAllByAuthorId(string authorId, int count)
    {
        return await session.Query<Tweed, TweedsByAuthorIdAndCreatedAt>()
            .Where(t => t.AuthorId == authorId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .Include(t => t.AuthorId).ToListAsync();
    }

    public async Task<List<Tweed>> Search(string term)
    {
        return await session.Query<Tweed, TweedsByText>()
            .Search(t => t.Text, term)
            .Take(20).ToListAsync();
    }

    public async Task<List<Tweed>> GetFollowerTweeds(List<string> followedUserIds,
        int count)
    {
        var followerTweeds = await session
            .Query<Tweed, TweedsByAuthorIdAndCreatedAt>()
            .Where(t => t.AuthorId.In(followedUserIds))
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .Include(t => t.AuthorId)
            .ToListAsync();
        return followerTweeds;
    }

    public async Task<List<Tweed>> GetUpTo20ReplyTweeds(IReadOnlyCollection<string> leadingTweedIds)
    {
        var prefix = string.Join(",", leadingTweedIds);
        var replyTweeds = await session.Query<TweedsByLeadingTweedIdsString.Result, TweedsByLeadingTweedIdsString>()
            .Where(t => t.LeadingTweedIdsString.StartsWith(prefix))
            .OfType<Tweed>()
            .Take(20)
            .Include(t => t.AuthorId)
            .ToListAsync();
        return replyTweeds;
    }

    public async Task<List<Tweed>> GetRecentTweeds(int count)
    {
        return await session.Query<Tweed>()
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .Include(t => t.AuthorId)
            .ToListAsync();
    }
}
