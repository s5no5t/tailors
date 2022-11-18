using NodaTime;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace Tweed.Data;

public interface ITweedQueries
{
    Task CreateTweed(Models.Tweed tweed);
    Task<IEnumerable<Models.Tweed>> GetLatestTweeds();
}

public sealed class TweedQueries : ITweedQueries
{
    private readonly IAsyncDocumentSession _session;

    public TweedQueries(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task CreateTweed(Models.Tweed tweed)
    {
        var now = SystemClock.Instance.GetCurrentInstant().InUtc();
        tweed.CreatedAt = now;
        await _session.StoreAsync(tweed);
    }

    public async Task<IEnumerable<Models.Tweed>> GetLatestTweeds()
    {
        return await _session.Query<Models.Tweed>().OrderByDescending(t => t.CreatedAt).ToListAsync();
    }
}
