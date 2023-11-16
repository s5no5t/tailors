using Raven.Client.Documents;
using Tailors.Domain.TweedAggregate;
using Tailors.Infrastructure.Test.Helper;
using Tailors.Infrastructure.UserLikesAggregate;

namespace Tailors.Infrastructure.Test.UserLikesAggregate;

[Trait("Category", "Integration")]
[Collection("RavenDB")]
public class UserLikesRepositoryTest(RavenTestDbFixture ravenDb)
{
    private readonly IDocumentStore _store = ravenDb.CreateDocumentStore();

    [Fact]
    public async Task GetLikesCount_ShouldReturn1_WhenTweedHasLike()
    {
        using var session = _store.OpenAsyncSession();
        var tweed = new Tweed(id: "tweedId", text: string.Empty, authorId: "authorId",
            createdAt: TestData.FixedDateTime);
        await session.StoreAsync(tweed);
        session.CountersFor(tweed.Id).Increment("Likes");
        await session.SaveChangesAsync();
        var sut = new UserLikesRepository(session);

        var likesCount = await sut.GetLikesCounter(tweed.Id!);

        Assert.Equal(1, likesCount);
    }
}
