using System.Collections.Generic;
using System.Threading.Tasks;
using NodaTime;
using Raven.Client.Documents;
using Tweed.Data.Entities;
using Xunit;

namespace Tweed.Data.Test;

[Collection("RavenDb Collection")]
public class AppUserQueriesTest
{
    private static readonly ZonedDateTime FixedZonedDateTime =
        new(new LocalDateTime(2022, 11, 18, 15, 20), DateTimeZone.Utc, new Offset());

    private readonly IDocumentStore _store;

    public AppUserQueriesTest(RavenTestDbFixture ravenDb)
    {
        _store = ravenDb.CreateDocumentStore();
    }

    [Fact]
    public async Task Search_ShouldReturnEmptyList_WhenNoResults()
    {
        using var session = _store.OpenAsyncSession();
        await session.StoreAsync(new AppUser
        {
            UserName = "UserName"
        });
        await session.SaveChangesAsync();
        AppUserQueries queries = new(session);

        var results = await queries.Search("noresults");

        Assert.Empty(results);
    }

    [Fact]
    public async Task Search_ShouldFindMatchingAppUser()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        await session.StoreAsync(new AppUser
        {
            UserName = "UserName"
        });
        await session.SaveChangesAsync();
        AppUserQueries queries = new(session);

        var results = await queries.Search("UserName");

        Assert.Contains(results, u => u.UserName == "UserName");
    }

    [Fact]
    public async Task Search_ShouldFindMatchingAppUser_WhenUserNamePrefixGiven()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        await session.StoreAsync(new AppUser
        {
            UserName = "UserName"
        });
        await session.SaveChangesAsync();
        AppUserQueries queries = new(session);

        var results = await queries.Search("Use");

        Assert.Contains(results, u => u.UserName == "UserName");
    }

    [Fact]
    public async Task Search_ShouldReturn20Users()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        for (var i = 0; i < 21; i++)
        {
            await session.StoreAsync(new AppUser
            {
                UserName = $"User-{i}"
            });
            await session.SaveChangesAsync();
        }

        AppUserQueries queries = new(session);

        var results = await queries.Search("User");

        Assert.Equal(20, results.Count);
    }

    [Fact]
    public async Task AddLike_ShouldIncreaseLikes()
    {
        using var session = _store.OpenAsyncSession();
        var appUser = new AppUser
        {
            Id = "currentUser"
        };
        await session.StoreAsync(appUser);
        var tweed = new Entities.Tweed
        {
            Id = "tweedId"
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var queries = new AppUserQueries(session);

        await queries.AddLike("tweedId", "currentUser", FixedZonedDateTime);

        Assert.Single(appUser.Likes);
    }

    [Fact]
    public async Task AddLike_ShouldIncreaseLikesCounter()
    {
        using var session = _store.OpenAsyncSession();
        var appUser = new AppUser
        {
            Id = "currentUser"
        };
        await session.StoreAsync(appUser);
        var tweed = new Entities.Tweed
        {
            Id = "tweedId"
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var queries = new AppUserQueries(session);

        await queries.AddLike("tweedId", "currentUser", FixedZonedDateTime);
        await session.SaveChangesAsync();

        var likesCounter = await session.CountersFor(tweed.Id).GetAsync("Likes");
        Assert.Equal(1, likesCounter);
    }

    [Fact]
    public async Task AddLike_ShouldNotIncreaseLikes_WhenUserHasAlreadyLiked()
    {
        using var session = _store.OpenAsyncSession();
        var appUser = new AppUser
        {
            Id = "currentUser",
            Likes = new List<TweedLike>
            {
                new()
                {
                    TweedId = "tweedId"
                }
            }
        };
        await session.StoreAsync(appUser);
        await session.SaveChangesAsync();
        var queries = new AppUserQueries(session);

        await queries.AddLike("tweedId", "currentUser", FixedZonedDateTime);

        Assert.Single(appUser.Likes);
    }

    [Fact]
    public async Task RemoveLike_ShouldDecreaseLikes()
    {
        using var session = _store.OpenAsyncSession();
        var appUser = new AppUser
        {
            Id = "currentUser",
            Likes = new List<TweedLike>
            {
                new()
                {
                    TweedId = "tweedId"
                }
            }
        };
        await session.StoreAsync(appUser);
        var tweed = new Entities.Tweed
        {
            Id = "tweedId"
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var queries = new AppUserQueries(session);

        await queries.RemoveLike("tweedId", "currentUser");

        Assert.Empty(appUser.Likes);
    }

    [Fact]
    public async Task RemoveLike_ShouldDecreaseLikesCounter()
    {
        using var session = _store.OpenAsyncSession();
        var appUser = new AppUser
        {
            Id = "userId"
        };
        await session.StoreAsync(appUser);
        var tweed = new Entities.Tweed
        {
            Id = "tweedId"
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        session.CountersFor(tweed.Id).Increment("Likes");
        await session.SaveChangesAsync();
        var queries = new AppUserQueries(session);

        await queries.RemoveLike(tweed.Id, "userId");
        await session.SaveChangesAsync();

        var likesCounter = await session.CountersFor(tweed.Id).GetAsync("Likes");
        Assert.Equal(0, likesCounter);
    }

    [Fact]
    public async Task RemoveLike_ShouldNotDecreaseLikes_WhenUserAlreadyDoesntLike()
    {
        using var session = _store.OpenAsyncSession();
        var appUser = new AppUser
        {
            Id = "userId"
        };
        await session.StoreAsync(appUser);
        var tweed = new Entities.Tweed
        {
            Id = "tweedId"
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var queries = new AppUserQueries(session);

        await queries.RemoveLike("tweedId", "userId");

        Assert.Empty(appUser.Likes);
    }
}
