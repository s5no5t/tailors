using JetBrains.Annotations;
using Raven.Client.Documents;
using Tailors.Domain.TweedAggregate;
using Tailors.Infrastructure.Test.Helper;
using Tailors.Infrastructure.TweedAggregate;

namespace Tailors.Infrastructure.Test.TweedAggregate;

[Trait("Category", "Integration")]
[Collection("RavenDB")]
public class TweedRepositoryTest(RavenTestDbFixture ravenDb) : IClassFixture<RavenTestDbFixture>
{
    private static readonly DateTime FixedDateTime = new(2022, 11, 18, 15, 20, 0);
    private readonly IDocumentStore _store = ravenDb.CreateDocumentStore();

    [Fact]
    public async Task GetById_ShouldReturnTweed()
    {
        using var session = _store.OpenAsyncSession();
        Tweed tweed = new(text: "test", createdAt: FixedDateTime, authorId: "authorId");
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var repository = new TweedRepository(session);

        var tweed2 = await repository.GetById(tweed.Id!);
        tweed2.Switch(
            [AssertionMethod] (t) => { Assert.Equal(tweed.Id, t.Id); }, _ => Assert.Fail());
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnNone()
    {
        using var session = _store.OpenAsyncSession();
        var repository = new TweedRepository(session);

        var tweed = await repository.GetById("invalid");
        tweed.Switch(
            _ => Assert.Fail("Should not return a tweed"),
            _ => { });
    }

    [Fact]
    public async Task GetTweedsForUser_ShouldReturnTweeds()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        Tweed tweed = new("user", "test", FixedDateTime);
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
        Tweed olderTweed = new("user1", "older tweed", FixedDateTime);
        await session.StoreAsync(olderTweed);
        var recent = FixedDateTime.AddHours(1);
        Tweed recentTweed = new("user1", "recent tweed", recent);
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
            Tweed tweed = new("user1", "test", FixedDateTime);
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
        Tweed tweed = new("user2", "test", FixedDateTime);
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

        var tweeds = await repository.Search("no-results");

        Assert.Empty(tweeds);
    }

    [Fact]
    public async Task Search_ShouldReturnResult_WhenTermIsFound()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        Tweed tweed = new(text: "Here is a word included.", id: "tweedId", createdAt: FixedDateTime,
            authorId: "authorId");
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var repository = new TweedRepository(session);

        var tweeds = await repository.Search("word");

        Assert.Equal("tweedId", tweeds[0].Id);
    }
}
