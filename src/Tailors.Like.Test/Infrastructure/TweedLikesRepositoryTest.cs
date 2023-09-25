using Raven.Client.Documents;
using Tailors.Like.Infrastructure;
using Tailors.Like.Test.Helper;
using Tailors.Thread.Domain.TweedAggregate;
using Tailors.Thread.Infrastructure;
using Xunit;

namespace Tailors.Like.Test.Infrastructure;

[Trait("Category","Integration")]
[Collection("RavenDB")]
public class TweedLikesRepositoryTest
{
    private static readonly DateTime FixedDateTime = new DateTime(2022, 11, 18, 15, 20, 0);

    private readonly IDocumentStore _store;

    public TweedLikesRepositoryTest(RavenTestDbFixture ravenDb)
    {
        _store = ravenDb.CreateDocumentStore();
    }

    [Fact]
    public async Task GetLikesCount_ShouldReturn1_WhenTweedHasLike()
    {
        using var session = _store.OpenAsyncSession();
        Tweed tweed = new()
        {
            Text = "test",
            CreatedAt = FixedDateTime
        };
        await session.StoreAsync(tweed);
        session.CountersFor(tweed.Id).Increment("Likes");
        await session.SaveChangesAsync();
        var service = new TweedLikesRepository(session);

        var likesCount = await service.GetLikesCounter(tweed.Id!);

        Assert.Equal(1, likesCount);
    }
}
