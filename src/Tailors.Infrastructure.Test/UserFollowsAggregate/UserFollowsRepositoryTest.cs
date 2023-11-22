using Tailors.Domain.UserAggregate;
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

        AppUser leader = new();
        await session.StoreAsync(leader);
        for (var i = 0; i < givenFollowerCount; i++)
        {
            AppUser follower = new();
            await session.StoreAsync(follower);

            UserFollows userFollows = new(follower.Id!);
            userFollows.AddFollows(leader.Id!, DateTime.UtcNow);
            await session.StoreAsync(userFollows);
        }

        await session.SaveChangesAsync();
        UserFollowsRepository repository = new(session);

        var followerCount = await repository.GetFollowerCount(leader.Id!);

        Assert.Equal(givenFollowerCount, followerCount);
    }
}
