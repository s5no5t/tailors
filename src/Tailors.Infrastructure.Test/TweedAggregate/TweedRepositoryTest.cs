using JetBrains.Annotations;
using Tailors.Domain.TweedAggregate;
using Tailors.Infrastructure.Test.Helper;
using Tailors.Infrastructure.TweedAggregate;

namespace Tailors.Infrastructure.Test.TweedAggregate;

[Trait("Category", "Integration")]
[Collection("RavenDB")]
public class TweedRepositoryTest(RavenTestDbFixture ravenDb) : IClassFixture<RavenTestDbFixture>
{
    [Fact]
    public async Task GetById_ShouldReturnTweed()
    {
        using var session = ravenDb.DocumentStore.OpenAsyncSession();
        Tweed tweed = new(text: "test", createdAt: TestData.FixedDateTime, authorId: "authorId");
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
        using var session = ravenDb.DocumentStore.OpenAsyncSession();
        var repository = new TweedRepository(session);

        var tweed = await repository.GetById("invalid");
        tweed.Switch(
            _ => Assert.Fail("Should not return a tweed"),
            _ => { });
    }

    [Fact]
    public async Task GetTweedsForUser_ShouldReturnTweeds()
    {
        using var session = ravenDb.DocumentStore.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        Tweed tweed = new("user1", "test", TestData.FixedDateTime);
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var repository = new TweedRepository(session);

        var tweeds = await repository.GetAllByAuthorId("user1", 10);

        Assert.NotEmpty(tweeds);
    }

    [Fact]
    public async Task GetTweedsForUser_ShouldReturnOrderedTweeds()
    {
        using var session = ravenDb.DocumentStore.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        Tweed olderTweed = new("user2", "older tweed", TestData.FixedDateTime);
        await session.StoreAsync(olderTweed);
        var recent = TestData.FixedDateTime.AddHours(1);
        Tweed recentTweed = new("user2", "recent tweed", recent);
        await session.StoreAsync(recentTweed);
        await session.SaveChangesAsync();
        var repository = new TweedRepository(session);

        var tweeds = await repository.GetAllByAuthorId("user2", 10);

        Assert.Equal(recentTweed, tweeds[0]);
        Assert.Equal(olderTweed, tweeds[1]);
    }

    [Fact]
    public async Task GetTweedsForUser_ShouldReturn20Tweeds()
    {
        using var session = ravenDb.DocumentStore.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        for (var i = 0; i < 25; i++)
        {
            Tweed tweed = new("user3", "test", TestData.FixedDateTime);
            await session.StoreAsync(tweed);
        }

        await session.SaveChangesAsync();
        var repository = new TweedRepository(session);

        var tweeds = await repository.GetAllByAuthorId("user3", 20);

        Assert.Equal(20, tweeds.Count);
    }

    [Fact]
    public async Task GetTweedsForUser_ShouldNotReturnTweedsFromOtherUsers()
    {
        using var session = ravenDb.DocumentStore.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();

        Tweed tweed = new("user4", "test", TestData.FixedDateTime);
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var repository = new TweedRepository(session);

        var tweeds = await repository.GetAllByAuthorId("userWithoutTweedsId", 10);

        Assert.Empty(tweeds);
    }

    [Fact]
    public async Task Search_ShouldReturnEmptyList_WhenNoResults()
    {
        using var session = ravenDb.DocumentStore.OpenAsyncSession();
        var repository = new TweedRepository(session);

        var tweeds = await repository.Search("no-results");

        Assert.Empty(tweeds);
    }

    [Fact]
    public async Task Search_ShouldReturnResult_WhenTermIsFound()
    {
        using var session = ravenDb.DocumentStore.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        Tweed tweed = new(text: "Here is a word included.", id: "tweedId", createdAt: TestData.FixedDateTime,
            authorId: "authorId");
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var repository = new TweedRepository(session);

        var tweeds = await repository.Search("word");

        Assert.Equal("tweedId", tweeds[0].Id);
    }
}
