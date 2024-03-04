using Tailors.Domain.UserFollowsAggregate;
using Tailors.Infrastructure.Test.Helper;
using Tailors.Infrastructure.UserFollowsAggregate;

namespace Tailors.Infrastructure.Test.UserFollowsAggregate;

[Trait("Category", "Integration")]
[Collection("RavenDB")]
public class UserFollowsRepositoryTest(RavenTestDbFixture ravenDb)
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    public async Task GetFollowerCount_ShouldReturnFollowerCount(int givenFollowerCount)
    {
        using var session = ravenDb.DocumentStore.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();

        for (var i = 0; i < givenFollowerCount; i++)
        {
            UserFollows userFollows = new($"followerId-{i}");
            userFollows.AddFollows($"leaderId-{givenFollowerCount}", DateTime.UtcNow);
            await session.StoreAsync(userFollows);
        }

        await session.SaveChangesAsync();
        UserFollowsRepository repository = new(session);

        var followerCount = await repository.GetFollowerCount($"leaderId-{givenFollowerCount}");

        Assert.Equal(givenFollowerCount, followerCount);
    }
}
