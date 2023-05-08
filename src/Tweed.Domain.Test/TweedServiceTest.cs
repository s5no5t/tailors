using System.Threading.Tasks;
using Moq;
using NodaTime;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Tweed.Domain.Model;
using Tweed.Domain.Test.Helper;
using Xunit;

namespace Tweed.Domain.Test;

[Collection("RavenDb Collection")]
public class TweedServiceTest : IClassFixture<RavenTestDbFixture>
{
    private static readonly ZonedDateTime FixedZonedDateTime =
        new(new LocalDateTime(2022, 11, 18, 15, 20), DateTimeZone.Utc, new Offset());

    private readonly IDocumentStore _store;

    public TweedServiceTest(RavenTestDbFixture ravenDb)
    {
        _store = ravenDb.CreateDocumentStore();
    }

    [Fact]
    public async Task StoreTweed_SavesTweed()
    {
        var session = new Mock<IAsyncDocumentSession>();
        var service = new TweedService(session.Object);

        await service.CreateTweed(new Tweed.Domain.Model.Tweed());

        session.Verify(s => s.StoreAsync(It.IsAny<Tweed.Domain.Model.Tweed>(), default));
    }

    [Fact]
    public async Task StoreTweed_CreatesThread_WhenTweedIsRoot()
    {
        var session = new Mock<IAsyncDocumentSession>();
        var service = new TweedService(session.Object);

        await service.CreateTweed(new Tweed.Domain.Model.Tweed());

        session.Verify(s => s.StoreAsync(It.IsAny<TweedThread>(), default));
    }

    [Fact]
    public async Task StoreTweed_UpdatesThreadId_WhenTweedHasParent()
    {
        var session = _store.OpenAsyncSession();
        var service = new TweedService(session);

        Tweed.Domain.Model.Tweed parentTweed = new()
        {
            Id = "parentTweedId",
            ThreadId = "threadId"
        };
        await session.StoreAsync(parentTweed);
        Tweed.Domain.Model.Tweed tweed = new()
        {
            ParentTweedId = "parentTweedId"
        };
        await service.CreateTweed(tweed);

        Assert.Equal(parentTweed.ThreadId, tweed.ThreadId);
    }

    [Fact]
    public async Task GetById_ShouldReturnTweed()
    {
        using var session = _store.OpenAsyncSession();
        Tweed.Domain.Model.Tweed tweed = new()
        {
            Text = "test",
            CreatedAt = FixedZonedDateTime
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var service = new TweedService(session);

        var tweed2 = await service.GetById(tweed.Id!);

        Assert.Equal(tweed.Id, tweed2?.Id);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnNull()
    {
        using var session = _store.OpenAsyncSession();
        var service = new TweedService(session);

        var tweed = await service.GetById("invalid");

        Assert.Null(tweed);
    }

    [Fact]
    public async Task GetTweedsForUser_ShouldReturnTweeds()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        Tweed.Domain.Model.Tweed tweed = new()
        {
            Text = "test",
            AuthorId = "user"
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var service = new TweedService(session);

        var tweeds = await service.GetTweedsForUser("user");

        Assert.NotEmpty(tweeds);
    }

    [Fact]
    public async Task GetTweedsForUser_ShouldReturnOrderedTweeds()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        Tweed.Domain.Model.Tweed olderTweed = new()
        {
            Text = "older tweed",
            CreatedAt = FixedZonedDateTime,
            AuthorId = "user1"
        };
        await session.StoreAsync(olderTweed);
        var recent = FixedZonedDateTime.PlusHours(1);
        Tweed.Domain.Model.Tweed recentTweed = new()
        {
            Text = "recent tweed",
            CreatedAt = recent,
            AuthorId = "user1"
        };
        await session.StoreAsync(recentTweed);
        await session.SaveChangesAsync();
        var service = new TweedService(session);

        var tweeds = await service.GetTweedsForUser("user1");

        Assert.Equal(recentTweed, tweeds[0]);
        Assert.Equal(olderTweed, tweeds[1]);
    }

    [Fact]
    public async Task GetTweedsForUser_ShouldReturn20Tweeds()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        for (var i = 0; i < 25; i++)
        {
            Tweed.Domain.Model.Tweed tweed = new()
            {
                Text = "test",
                CreatedAt = FixedZonedDateTime,
                AuthorId = "user1"
            };
            await session.StoreAsync(tweed);
        }

        await session.SaveChangesAsync();
        var service = new TweedService(session);

        var tweeds = await service.GetTweedsForUser("user1");

        Assert.Equal(20, tweeds.Count);
    }

    [Fact]
    public async Task GetTweedsForUser_ShouldNotReturnTweedsFromOtherUsers()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        Tweed.Domain.Model.Tweed tweed = new()
        {
            Text = "test",
            CreatedAt = FixedZonedDateTime,
            AuthorId = "user2"
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var service = new TweedService(session);

        var tweeds = await service.GetTweedsForUser("user1");

        Assert.Empty(tweeds);
    }
}
