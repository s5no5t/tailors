using NodaTime;
using Raven.Client.Documents;
using Tweed.Domain.Model;
using Tweed.Infrastructure;
using Tweed.Like.Infrastructure;
using Tweed.Like.Test.Helper;
using Tweed.Tweed.Domain;
using Xunit;

namespace Tweed.Like.Test.Infrastructure;

[Collection("RavenDB")]
public class TweedLikesRepositoryTest
{
    private static readonly ZonedDateTime FixedZonedDateTime =
        new(new LocalDateTime(2022, 11, 18, 15, 20), DateTimeZone.Utc, new Offset());

    private readonly IDocumentStore _store;

    public TweedLikesRepositoryTest(RavenTestDbFixture ravenDb)
    {
        _store = ravenDb.CreateDocumentStore();
    }

    [Fact]
    public async Task GetLikesCount_ShouldReturn1_WhenTweedHasLike()
    {
        using var session = _store.OpenAsyncSession();
        TheTweed tweed = new()
        {
            Text = "test",
            CreatedAt = FixedZonedDateTime
        };
        await session.StoreAsync(tweed);
        session.CountersFor(tweed.Id).Increment("Likes");
        await session.SaveChangesAsync();
        var service = new TweedLikesRepository(session);

        var likesCount = await service.GetLikesCounter(tweed.Id!);

        Assert.Equal(1, likesCount);
    }
}
