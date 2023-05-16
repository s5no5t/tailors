using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Tweed.User.Domain;
using Tweed.User.Infrastructure;
using Tweed.User.Test.Helper;
using Xunit;

namespace Tweed.User.Test.Infrastructure;

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

        User.Domain.AppUser leader = new()
        {
            Id = "leaderId"
        };
        await session.StoreAsync(leader);
        for (var i = 0; i < givenFollowerCount; i++)
        {
            User.Domain.AppUser follower = new()
            {
                Id = $"follower/${i}"
            };
            await session.StoreAsync(follower);
            UserFollows userFollows = new()
            {
                UserId = follower.Id,
                Follows = new List<UserFollows.LeaderReference>
                {
                    new()
                    {
                        LeaderId = "leaderId"
                    }
                }
            };
            await session.StoreAsync(userFollows);
        }

        await session.SaveChangesAsync();
        UserFollowsRepository repository = new(session);

        var followerCount = await repository.GetFollowerCount("leaderId");

        Assert.Equal(givenFollowerCount, followerCount);
    }
}
