using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NodaTime;
using Raven.Client.Documents;
using Tweed.Domain.Model;
using Tweed.Domain.Test.Helper;
using Xunit;

namespace Tweed.Domain.Test;

[Collection("RavenDb Collection")]
public class FeedBuilderServiceTest : IAsyncLifetime
{
    private static readonly ZonedDateTime FixedZonedDateTime =
        new(new LocalDateTime(2022, 11, 18, 15, 20), DateTimeZone.Utc, new Offset());

    private readonly AppUser _currentUser = new();

    private readonly IDocumentStore _store;

    public FeedBuilderServiceTest(RavenTestDbFixture ravenDb)
    {
        _store = ravenDb.CreateDocumentStore();
    }

    public async Task InitializeAsync()
    {
        await using var bulkInsert = _store.BulkInsert();

        for (var i = 0; i < 5; i++)
        {
            Tweed.Domain.Model.Tweed currentUserTweed = new()
            {
                Text = "test",
                AuthorId = _currentUser.Id!,
                CreatedAt = FixedZonedDateTime.PlusHours(-100)
            };
            await bulkInsert.StoreAsync(currentUserTweed);
        }

        List<AppUser> otherUsers = new();
        for (var i = 0; i < 5; i++)
        {
            AppUser otherUser = new();
            await bulkInsert.StoreAsync(otherUser);
            otherUsers.Add(otherUser);

            for (var j = 0; j < 20; j++)
            {
                Tweed.Domain.Model.Tweed otherUserTweed = new()
                {
                    Text = "test",
                    AuthorId = otherUser.Id!,
                    CreatedAt = FixedZonedDateTime.PlusHours(-100)
                };
                await bulkInsert.StoreAsync(otherUserTweed);
            }
        }

        await bulkInsert.StoreAsync(_currentUser);

        AppUserFollows currentUserFollows = new()
        {
            AppUserId = _currentUser.Id,
            Follows = otherUsers.Take(3).Select(u => new AppUserFollows.LeaderReference
            {
                LeaderId = u.Id,
                CreatedAt = FixedZonedDateTime.PlusHours(-100)
            }).ToList()
        };
        await bulkInsert.StoreAsync(currentUserFollows);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetFeed_ShouldReturnTweedsByCurrentUser()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();

        Tweed.Domain.Model.Tweed currentUserTweed = new()
        {
            Text = "test",
            AuthorId = _currentUser.Id!,
            CreatedAt = FixedZonedDateTime.PlusHours(1)
        };
        await session.StoreAsync(currentUserTweed);

        await session.SaveChangesAsync();
        AppUserFollowsService appUserFollowsService = new(session);
        var service = new FeedService(session, appUserFollowsService);

        var tweeds = await service.GetFeed(_currentUser.Id!, 0);

        Assert.Contains(currentUserTweed.Id, tweeds.Select(t => t.Id));
    }

    [Fact]
    public async Task GetFeed_ShouldReturnTweedsByFollowedUsers()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();

        var followedUser = new AppUser();
        await session.StoreAsync(followedUser);

        AppUserFollows currentUserFollows = new()
        {
            AppUserId = _currentUser.Id,
            Follows = new List<AppUserFollows.LeaderReference>
            {
                new()
                {
                    LeaderId = followedUser.Id!
                }
            }
        };
        await session.StoreAsync(currentUserFollows);

        Tweed.Domain.Model.Tweed followedUserTweed = new()
        {
            Text = "test",
            AuthorId = followedUser.Id!,
            CreatedAt = FixedZonedDateTime.PlusHours(1)
        };
        await session.StoreAsync(followedUserTweed);

        await session.SaveChangesAsync();
        AppUserFollowsService appUserFollowsService = new(session);
        var service = new FeedService(session, appUserFollowsService);

        var tweeds = await service.GetFeed(_currentUser.Id!, 0);

        Assert.Contains(tweeds, t => t.Id == followedUserTweed.Id);
    }

    [Fact]
    public async Task GetFeed_ShouldNotReturnTheSameTweedTwice()
    {
        _store.OnBeforeQuery += (sender, beforeQueryExecutedArgs) =>
        {
            beforeQueryExecutedArgs.QueryCustomization.WaitForNonStaleResults();
        };
        using var session = _store.OpenAsyncSession();

        AppUserFollowsService appUserFollowsService = new(session);
        var service = new FeedService(session, appUserFollowsService);

        var tweeds = (await service.GetFeed(_currentUser.Id!, 0)).ToList();

        var anyDuplicateTweed = tweeds.GroupBy(x => x.Id).Any(g => g.Count() > 1);
        Assert.False(anyDuplicateTweed);
    }

    [Fact]
    public async Task GetFeed_ShouldReturnPageSizeTweeds()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();

        for (var i = 0; i < 25; i++)
        {
            Tweed.Domain.Model.Tweed tweed = new()
            {
                Text = "test",
                AuthorId = _currentUser.Id!,
                CreatedAt = FixedZonedDateTime
            };
            await session.StoreAsync(tweed);
        }

        await session.SaveChangesAsync();
        AppUserFollowsService appUserFollowsService = new(session);
        var service = new FeedService(session, appUserFollowsService);

        var feed = (await service.GetFeed(_currentUser.Id!, 0)).ToList();

        Assert.Equal(FeedService.PageSize, feed.Count);
    }

    [Fact]
    public async Task GetFeed_ShouldRespectPage()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();

        AppUserFollowsService appUserFollowsService = new(session);
        var service = new FeedService(session, appUserFollowsService);

        var page0Feed = (await service.GetFeed(_currentUser.Id!, 0)).ToList();
        var page1Feed = (await service.GetFeed(_currentUser.Id!, 1)).ToList();

        Assert.NotEqual(page0Feed[0].Id, page1Feed[1].Id);
    }
}
