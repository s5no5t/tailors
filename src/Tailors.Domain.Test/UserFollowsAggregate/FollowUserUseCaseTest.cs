using Tailors.Domain.UserFollowsAggregate;

namespace Tailors.Domain.Test.UserFollowsAggregate;

public class FollowUserUseCaseTest
{
    private readonly FollowUserUseCase _sut;

    private readonly UserFollowsRepositoryMock _userFollowsRepositoryMock = new();

    public FollowUserUseCaseTest()
    {
        _sut = new FollowUserUseCase(_userFollowsRepositoryMock);
    }

    [Fact]
    public async Task AddFollower_ShouldAddFollower()
    {
        UserFollows userFollows = new("followerId");
        await _userFollowsRepositoryMock.Create(userFollows);

        await _sut.AddFollower("leaderId", userFollows.UserId, TestData.FixedDateTime);

        Assert.Equal("leaderId", userFollows.Follows[0].LeaderId);
    }

    [Fact]
    public async Task AddFollower_ShouldNotAddFollower_WhenAlreadyFollowed()
    {
        UserFollows userFollows = new("followerId");
        userFollows.AddFollows("leaderId", TestData.FixedDateTime);
        await _userFollowsRepositoryMock.Create(userFollows);

        await _sut.AddFollower("leaderId", userFollows.UserId, TestData.FixedDateTime);

        Assert.Single(userFollows.Follows);
    }

    [Fact]
    public async Task RemoveFollower_ShouldRemoveFollower()
    {
        UserFollows userFollows = new("followerId");
        userFollows.AddFollows("leaderId", TestData.FixedDateTime);
        await _userFollowsRepositoryMock.Create(userFollows);

        await _sut.RemoveFollower("leaderId", "followerId");

        Assert.DoesNotContain(userFollows.Follows, u => u.LeaderId == "leaderId");
    }
}
