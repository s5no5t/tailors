using NodaTime;
using Raven.Client.Documents;
using Tweed.Infrastructure.Test.Helper;
using Xunit;

namespace Tweed.Infrastructure.Test;

[Collection("RavenDb Collection")]
public class TweedRepositoryTest : IClassFixture<RavenTestDbFixture>
{
    private static readonly ZonedDateTime FixedZonedDateTime =
        new(new LocalDateTime(2022, 11, 18, 15, 20), DateTimeZone.Utc, new Offset());

    private readonly IDocumentStore _store;

    public TweedRepositoryTest(RavenTestDbFixture ravenDb)
    {
        _store = ravenDb.CreateDocumentStore();
    }

    [Fact]
    public async Task GetById_ShouldReturnTweed()
    {
        using var session = _store.OpenAsyncSession();
        Domain.Model.Tweed tweed = new()
        {
            Text = "test",
            CreatedAt = FixedZonedDateTime
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var repository = new TweedRepository(session);

        var tweed2 = await repository.GetById(tweed.Id!);

        Assert.Equal(tweed.Id, tweed2?.Id);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnNull()
    {
        using var session = _store.OpenAsyncSession();
        var repository = new TweedRepository(session);

        var tweed = await repository.GetById("invalid");

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
        var repository = new TweedRepository(session);

        var tweeds = await repository.GetAllByAuthorId("user", 10);

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
        var repository = new TweedRepository(session);

        var tweeds = await repository.GetAllByAuthorId("user1", 10);

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
        var repository = new TweedRepository(session);

        var tweeds = await repository.GetAllByAuthorId("user1", 20);

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
        var repository = new TweedRepository(session);

        var tweeds = await repository.GetAllByAuthorId("user1", 10);

        Assert.Empty(tweeds);
    }

    [Fact]
    public async Task Search_ShouldReturnEmptyList_WhenNoResults()
    {
        using var session = _store.OpenAsyncSession();
        var repository = new TweedRepository(session);

        var tweeds = await repository.Search("noresults");

        Assert.Empty(tweeds);
    }

    [Fact]
    public async Task Search_ShouldReturnResult_WhenTermIsFound()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        Domain.Model.Tweed tweed = new()
        {
            Id = "tweedId",
            Text = "Here is a word included."
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var repository = new TweedRepository(session);

        var tweeds = await repository.Search("word");

        Assert.Equal("tweedId", tweeds[0].Id);
    }
}
