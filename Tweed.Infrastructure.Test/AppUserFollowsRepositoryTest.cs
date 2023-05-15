using Raven.Client.Documents;
using Tweed.Domain.Model;
using Tweed.Infrastructure.Test.Helper;
using Xunit;

namespace Tweed.Infrastructure.Test;

[Collection("RavenDB")]
public class AppUserFollowsRepositoryTest
{
    private readonly IDocumentStore _store;

    public AppUserFollowsRepositoryTest(RavenTestDbFixture ravenDb)
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
            AppUserFollows appUserFollows = new()
            {
                AppUserId = follower.Id,
                Follows = new List<AppUserFollows.LeaderReference>
                {
                    new()
                    {
                        LeaderId = "leaderId"
                    }
                }
            };
            await session.StoreAsync(appUserFollows);
        }

        await session.SaveChangesAsync();
        AppUserFollowsRepository repository = new(session);

        var followerCount = await repository.GetFollowerCount("leaderId");

        Assert.Equal(givenFollowerCount, followerCount);
    }
}
