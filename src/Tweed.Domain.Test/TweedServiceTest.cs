using System.Threading.Tasks;
using Moq;
using NodaTime;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Tweed.Domain.Model;
using Tweed.Domain.Test.Helper;
using Tweed.Infrastructure;
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
    public async Task CreateRootTweed_SavesTweed()
    {
        var session = new Mock<IAsyncDocumentSession>();
        var service = new TweedService(session.Object);

        await service.CreateRootTweed("authorId", "text", FixedZonedDateTime);

        session.Verify(s => s.StoreAsync(It.IsAny<Domain.Model.Tweed>(), default));
    }

    [Fact]
    public async Task CreateRootTweed_CreatesThread()
    {
        var session = new Mock<IAsyncDocumentSession>();
        var service = new TweedService(session.Object);

        await service.CreateRootTweed("authorId", "text", FixedZonedDateTime);

        session.Verify(s => s.StoreAsync(It.IsAny<TweedThread>(), default));
    }

    [Fact]
    public async Task CreateReplyTweed_SavesTweed()
    {
        var session = _store.OpenAsyncSession();
        var service = new TweedService(session);

        Domain.Model.Tweed parentTweed = new()
        {
            Id = "parentTweedId",
            ThreadId = "threadId"
        };
        await session.StoreAsync(parentTweed);

        var tweed =
            await service.CreateReplyTweed("authorId", "text", FixedZonedDateTime, parentTweed.Id);

        Assert.NotNull(tweed.Id);
    }

    [Fact]
    public async Task CreateReplyTweed_SetsThreadId()
    {
        var session = _store.OpenAsyncSession();
        var service = new TweedService(session);

        Domain.Model.Tweed parentTweed = new()
        {
            Id = "parentTweedId",
            ThreadId = "threadId"
        };
        await session.StoreAsync(parentTweed);

        var tweed =
            await service.CreateReplyTweed("authorId", "text", FixedZonedDateTime, "parentTweedId");

        Assert.Equal(parentTweed.ThreadId, tweed.ThreadId);
    }

    [Fact]
    public async Task GetTweedById_ShouldReturnTweed()
    {
        using var session = _store.OpenAsyncSession();
        Domain.Model.Tweed tweed = new()
        {
            Text = "test",
            CreatedAt = FixedZonedDateTime
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var service = new TweedService(session);

        var tweed2 = await service.GetTweedById(tweed.Id!);

        Assert.Equal(tweed.Id, tweed2?.Id);
    }

    [Fact]
    public async Task GetTweedById_WithInvalidId_ShouldReturnNull()
    {
        using var session = _store.OpenAsyncSession();
        var service = new TweedService(session);

        var tweed = await service.GetTweedById("invalid");

        Assert.Null(tweed);
    }

    [Fact]
    public async Task GetTweedsForUser_ShouldReturnTweeds()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        Domain.Model.Tweed tweed = new()
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
        Domain.Model.Tweed olderTweed = new()
        {
            Text = "older tweed",
            CreatedAt = FixedZonedDateTime,
            AuthorId = "user1"
        };
        await session.StoreAsync(olderTweed);
        var recent = FixedZonedDateTime.PlusHours(1);
        Domain.Model.Tweed recentTweed = new()
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
            Domain.Model.Tweed tweed = new()
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
        Domain.Model.Tweed tweed = new()
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
