using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace Tweed.Data;

public interface ITweedQueries
{
    Task SaveTweed(Models.Tweed tweed);
    Task<IEnumerable<Models.Tweed>> GetLatestTweeds();
}

public sealed class TweedQueries : ITweedQueries
{
    private readonly IAsyncDocumentSession _session;

    public TweedQueries(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task SaveTweed(Models.Tweed tweed)
    {
        await _session.StoreAsync(tweed);
    }

    public async Task<IEnumerable<Models.Tweed>> GetLatestTweeds()
    {
        return await _session.Query<Models.Tweed>().ToListAsync();
    }
}
