using Raven.Client.Documents;
using Tailors.Domain.Tweed;
using Tailors.Infrastructure.Test.Helper;
using Tailors.Infrastructure.Tweed;

namespace Tailors.Infrastructure.Test.Tweed;

[Trait("Category", "Integration")]
[Collection("RavenDB")]
public class TweedRepositoryTest : IClassFixture<RavenTestDbFixture>
{
    private static readonly DateTime FixedDateTime = new DateTime(2022, 11, 18, 15, 20, 0);
    private readonly IDocumentStore _store;

    public TweedRepositoryTest(RavenTestDbFixture ravenDb)
    {
        _store = ravenDb.CreateDocumentStore();
    }

    [Fact]
    public async Task GetById_ShouldReturnTweed()
    {
        using var session = _store.OpenAsyncSession();
        TailorsTweed tweed = new(text: "test", createdAt: FixedDateTime, authorId: "authorId");
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
        TailorsTweed tweed = new(authorId: "user", text: "test", createdAt: FixedDateTime);
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
        TailorsTweed olderTweed = new(authorId: "user1", text: "older tweed", createdAt: FixedDateTime);
        await session.StoreAsync(olderTweed);
        var recent = FixedDateTime.AddHours(1);
        TailorsTweed recentTweed = new(authorId: "user1", text: "recent tweed", createdAt: recent);
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
            TailorsTweed tweed = new(authorId: "user1", text: "test", createdAt: FixedDateTime);
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
        TailorsTweed tweed = new(authorId: "user2", text: "test", createdAt: FixedDateTime);
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
        TailorsTweed tweed = new(text: "Here is a word included.", id: "tweedId", createdAt: FixedDateTime, authorId: "authorId");
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var repository = new TweedRepository(session);

        var tweeds = await repository.Search("word");

        Assert.Equal("tweedId", tweeds[0].Id);
    }
}