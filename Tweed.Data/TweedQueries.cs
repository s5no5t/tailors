using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace Tweed.Data;

public interface ITweedQueries
{
    Task StoreTweed(Models.Tweed tweed);
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

    public async Task StoreTweed(Models.Tweed tweed)
    {
        if (tweed.CreatedAt is null)
            throw new ArgumentException("tweed.CreatedAt must not be null");
        if (tweed.AuthorId is null)
            throw new ArgumentException("tweed.AuthorId must not be null");

        await _session.StoreAsync(tweed);
    }
}
