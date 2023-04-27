using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NodaTime;
using Raven.Client.Documents;
using Tweed.Data.Model;
using Xunit;

namespace Tweed.Data.Test;

[Collection("RavenDb Collection")]
public class FeedBuilderTest
{
    private static readonly ZonedDateTime FixedZonedDateTime =
        new(new LocalDateTime(2022, 11, 18, 15, 20), DateTimeZone.Utc, new Offset());
    private readonly IDocumentStore _store;

    public FeedBuilderTest(RavenTestDbFixture ravenDb)
    {
        _store = ravenDb.CreateDocumentStore();
    }
    
    [Fact]
    public async Task GetFeed_ShouldReturnTweedsByCurrentUser()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        var currentUser = new AppUser
        {
            Id = "currentUser"
        };
        await session.StoreAsync(currentUser);
        Model.Tweed currentUserTweed = new()
        {
            Text = "test",
            AuthorId = "currentUser"
        };
        await session.StoreAsync(currentUserTweed);
        Model.Tweed otherUserTweed = new()
        {
            Text = "test",
            AuthorId = "otherUser"
        };
        await session.StoreAsync(otherUserTweed);
        await session.SaveChangesAsync();
        AppUserFollowsQueries appUserFollowsQueries = new(session);
        var queries = new FeedBuilder(session, appUserFollowsQueries);

        var tweeds = await queries.GetFeed("currentUser", 0);

        Assert.Contains(currentUserTweed.Id, tweeds.Select(t => t.Id));
    }

    [Fact]
    public async Task GetFeed_ShouldReturnTweedsByFollowedUsers()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        var currentUser = new AppUser
        {
            Id = "currentUser",
        };
        await session.StoreAsync(currentUser);
        AppUserFollows currentUserFollows = new()
        {
            AppUserId = currentUser.Id,
            Follows = new List<Follows>
            {
                new()
                {
                    LeaderId = "followedUser"
                }
            }
        };
        await session.StoreAsync(currentUserFollows);
        var followedUser = new AppUser
        {
            Id = "followedUser"
        };
        await session.StoreAsync(followedUser);
        Model.Tweed followedUserTweed = new()
        {
            Text = "test",
            AuthorId = "followedUser"
        };
        await session.StoreAsync(followedUserTweed);
        Model.Tweed notFollowedUserTweed = new()
        {
            Text = "test",
            AuthorId = "notFollowedUser"
        };
        await session.StoreAsync(notFollowedUserTweed);
        await session.SaveChangesAsync();
        AppUserFollowsQueries appUserFollowsQueries = new(session);
        var queries = new FeedBuilder(session, appUserFollowsQueries);

        var tweeds = await queries.GetFeed("currentUser", 0);

        Assert.Contains(tweeds, t => t.Id == followedUserTweed.Id);
    }

    [Fact]
    public async Task GetFeed_ShouldReturnOrderedTweeds()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        var currentUser = new AppUser
        {
            Id = "currentUser"
        };
        await session.StoreAsync(currentUser);
        Model.Tweed olderTweed = new()
        {
            Text = "older tweed",
            AuthorId = "currentUser",
            CreatedAt = FixedZonedDateTime
        };
        await session.StoreAsync(olderTweed);
        var recent = FixedZonedDateTime.PlusHours(1);
        Model.Tweed recentTweed = new()
        {
            Text = "recent tweed",
            AuthorId = "currentUser",
            CreatedAt = recent
        };
        await session.StoreAsync(recentTweed);
        await session.SaveChangesAsync();
        AppUserFollowsQueries appUserFollowsQueries = new(session);
        var queries = new FeedBuilder(session, appUserFollowsQueries);

        var tweeds = (await queries.GetFeed("currentUser", 0)).ToList();

        Assert.Equal(recentTweed.Id, tweeds[0].Id);
        Assert.Equal(olderTweed.Id, tweeds[1].Id);
    }

    [Fact]
    public async Task GetFeed_ShouldReturn20Tweeds()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        var currentUser = new AppUser
        {
            Id = "currentUser"
        };
        await session.StoreAsync(currentUser);
        for (var i = 0; i < 25; i++)
        {
            Model.Tweed tweed = new()
            {
                Text = "test",
                AuthorId = "currentUser",
                CreatedAt = FixedZonedDateTime
            };
            await session.StoreAsync(tweed);
        }

        await session.SaveChangesAsync();
        AppUserFollowsQueries appUserFollowsQueries = new(session);
        var queries = new FeedBuilder(session, appUserFollowsQueries);

        var tweeds = (await queries.GetFeed("currentUser", 0)).ToList();

        Assert.Equal(20, tweeds.Count);
    }
}