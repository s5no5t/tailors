using System.Diagnostics;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace Tweed.Data;

public interface ITweedQueries
{
    Task CreateTweed(Models.Tweed tweed, string? authorId = null);
    Task<IEnumerable<Models.Tweed>> GetLatestTweeds();
}

public sealed class TweedQueries : ITweedQueries
{
    private readonly IAsyncDocumentSession _session;

    public TweedQueries(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task<IEnumerable<Models.Tweed>> GetLatestTweeds()
    {
        return await _session.Query<Models.Tweed>().OrderByDescending(t => t.CreatedAt).Take(20).ToListAsync();
    }

    public async Task CreateTweed(Models.Tweed tweed, string? authorId)
    {
        Debug.Assert(tweed.CreatedAt != null);

        tweed.AuthorId = authorId;
        await _session.StoreAsync(tweed);
    }
}
