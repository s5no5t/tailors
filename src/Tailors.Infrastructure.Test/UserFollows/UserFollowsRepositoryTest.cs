using Raven.Client.Documents;
using Tailors.Domain.User;
using Tailors.Infrastructure.Test.Helper;
using Tailors.Infrastructure.UserFollows;

namespace Tailors.Infrastructure.Test.UserFollows;

[Trait("Category","Integration")]
[Collection("RavenDB")]
public class UserFollowsRepositoryTest
{
    private readonly IDocumentStore _store;

    public UserFollowsRepositoryTest(RavenTestDbFixture ravenDb)
    {
        _store = ravenDb.CreateDocumentStore();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    public async Task GetFollowerCount_ShouldReturnFollowerCount(int givenFollowerCount)
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();

        AppUser leader = new()
        {
            Id = "leaderId"
        };
        await session.StoreAsync(leader);
        for (var i = 0; i < givenFollowerCount; i++)
        {
            AppUser follower = new()
            {
                Id = $"follower/${i}"
            };
            await session.StoreAsync(follower);
            Domain.UserFollows.UserFollows userFollows = new(userId: follower.Id);
            userFollows.AddFollows(leaderId: "leaderId", createdAt: DateTime.UtcNow);
            await session.StoreAsync(userFollows);
        }

        await session.SaveChangesAsync();
        UserFollowsRepository repository = new(session);

        var followerCount = await repository.GetFollowerCount("leaderId");

        Assert.Equal(givenFollowerCount, followerCount);
    }
}
