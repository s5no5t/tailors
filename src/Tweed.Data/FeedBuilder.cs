using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Tweed.Data.Indexes;

namespace Tweed.Data;

public interface IFeedBuilder
{
    Task<List<Entities.Tweed>> GetFeed(string userId);
}

public class FeedBuilder : IFeedBuilder
{
    private readonly IAsyncDocumentSession _session;
    private readonly IAppUserFollowsQueries _appUserFollowsQueries;

    public FeedBuilder(IAsyncDocumentSession session, IAppUserFollowsQueries appUserFollowsQueries)
    {
        _session = session;
        _appUserFollowsQueries = appUserFollowsQueries;
    }

    public async Task<List<Entities.Tweed>> GetFeed(string userId)
    {
        var follows = await _appUserFollowsQueries.GetFollows(userId);
        var followedUserIds = follows.Select(f => f.LeaderId).ToList();

        followedUserIds.Add(userId); // show her own Tweeds as well

        var followerTweeds = await _session.Query<Entities.Tweed, Tweeds_ByAuthorIdAndCreatedAt>()
            .Where(t => t.AuthorId.In(followedUserIds))
            .OrderByDescending(t => t.CreatedAt)
            .Take(20)
            .ToListAsync();

        var numExtraTweeds = 20 - followerTweeds.Count;
        var extraTweeds = await _session.Query<Entities.Tweed>()
            .Where(t => !t.Id.In(followerTweeds.Select(f => f.Id).ToList())) // not Tweeds that are already in the feed
            .OrderByDescending(t => t.CreatedAt)
            .Take(numExtraTweeds)
            .ToListAsync();

        var tweeds = new List<Entities.Tweed>();
        tweeds.AddRange(followerTweeds);
        tweeds.AddRange(extraTweeds);
        tweeds = tweeds.OrderByDescending(t => t.CreatedAt?.LocalDateTime).ToList();
        return tweeds;
    }
}