using System.Collections.Generic;
using System.Threading.Tasks;
using NodaTime;
using Raven.Client.Documents;
using Tweed.Data.Domain;
using Tweed.Data.Model;
using Tweed.Data.Test.Helper;
using Xunit;

namespace Tweed.Data.Test.Domain;

[Collection("RavenDb Collection")]
public class TweedLikesServiceTest
{
    private static readonly ZonedDateTime FixedZonedDateTime =
        new(new LocalDateTime(2022, 11, 18, 15, 20), DateTimeZone.Utc, new Offset());

    private readonly IDocumentStore _store;

    public TweedLikesServiceTest(RavenTestDbFixture ravenDb)
    {
        _store = ravenDb.CreateDocumentStore();
    }

    [Fact]
    public async Task AddLike_ShouldIncreaseLikes()
    {
        using var session = _store.OpenAsyncSession();
        var appUserLikes = new AppUserLikes
        {
            AppUserId = "currentUser"
        };
        await session.StoreAsync(appUserLikes);
        var tweed = new Data.Model.Tweed
        {
            Id = "tweedId"
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var service = new TweedLikesService(session);

        await service.AddLike("tweedId", "currentUser", FixedZonedDateTime);

        Assert.Single(appUserLikes.Likes);
    }

    [Fact]
    public async Task AddLike_ShouldIncreaseLikesCounter()
    {
        using var session = _store.OpenAsyncSession();
        var appUserLikes = new AppUserLikes
        {
            AppUserId = "currentUser"
        };
        await session.StoreAsync(appUserLikes);
        var tweed = new Data.Model.Tweed
        {
            Id = "tweedId"
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var service = new TweedLikesService(session);

        await service.AddLike("tweedId", "currentUser", FixedZonedDateTime);
        await session.SaveChangesAsync();

        var likesCounter = await session.CountersFor(tweed.Id).GetAsync("Likes");
        Assert.Equal(1, likesCounter);
    }

    [Fact]
    public async Task AddLike_ShouldNotIncreaseLikes_WhenUserHasAlreadyLiked()
    {
        using var session = _store.OpenAsyncSession();
        var appUserLikes = new AppUserLikes
        {
            AppUserId = "currentUser",
            Likes = new List<AppUserLikes.TweedLike>
            {
                new()
                {
                    TweedId = "tweedId"
                }
            }
        };
        await session.StoreAsync(appUserLikes);
        await session.SaveChangesAsync();
        var service = new TweedLikesService(session);

        await service.AddLike("tweedId", "currentUser", FixedZonedDateTime);

        Assert.Single(appUserLikes.Likes);
    }

    [Fact]
    public async Task RemoveLike_ShouldDecreaseLikes()
    {
        using var session = _store.OpenAsyncSession();
        var appUserLikes = new AppUserLikes
        {
            AppUserId = "currentUser",
            Likes = new List<AppUserLikes.TweedLike>
            {
                new()
                {
                    TweedId = "tweedId"
                }
            }
        };
        await session.StoreAsync(appUserLikes);
        var tweed = new Data.Model.Tweed
        {
            Id = "tweedId"
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var service = new TweedLikesService(session);

        await service.RemoveLike("tweedId", "currentUser");

        Assert.Empty(appUserLikes.Likes);
    }

    [Fact]
    public async Task RemoveLike_ShouldDecreaseLikesCounter()
    {
        using var session = _store.OpenAsyncSession();
        var appUserLikes = new AppUserLikes
        {
            AppUserId = "userId"
        };
        await session.StoreAsync(appUserLikes);
        var tweed = new Data.Model.Tweed
        {
            Id = "tweedId"
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        session.CountersFor(tweed.Id).Increment("Likes");
        await session.SaveChangesAsync();
        var service = new TweedLikesService(session);

        await service.RemoveLike(tweed.Id, "userId");
        await session.SaveChangesAsync();

        var likesCounter = await session.CountersFor(tweed.Id).GetAsync("Likes");
        Assert.Equal(0, likesCounter);
    }

    [Fact]
    public async Task RemoveLike_ShouldNotDecreaseLikes_WhenUserAlreadyDoesntLike()
    {
        using var session = _store.OpenAsyncSession();
        var appUserLikes = new AppUserLikes
        {
            AppUserId = "userId"
        };
        await session.StoreAsync(appUserLikes);
        var tweed = new Data.Model.Tweed
        {
            Id = "tweedId"
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var service = new TweedLikesService(session);

        await service.RemoveLike("tweedId", "userId");

        Assert.Empty(appUserLikes.Likes);
    }

    [Fact]
    public async Task GetLikesCount_ShouldReturn1_WhenTweedHasLike()
    {
        using var session = _store.OpenAsyncSession();
        Data.Model.Tweed tweed = new()
        {
            Text = "test",
            CreatedAt = FixedZonedDateTime
        };
        await session.StoreAsync(tweed);
        session.CountersFor(tweed.Id).Increment("Likes");
        await session.SaveChangesAsync();
        var service = new TweedLikesService(session);

        var likesCount = await service.GetLikesCount(tweed.Id!);

        Assert.Equal(1, likesCount);
    }
}
