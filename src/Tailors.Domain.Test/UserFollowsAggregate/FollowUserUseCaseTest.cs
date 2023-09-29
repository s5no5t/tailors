using Moq;
using Tailors.Domain.UserFollowsAggregate;

namespace Tailors.Domain.Test.UserFollowsAggregate;

public class FollowUserUseCaseTest
{
    private static readonly DateTime FixedDateTime = new(2022, 11, 18, 15, 20, 0);

    private readonly Mock<IUserFollowsRepository> _userFollowsRepositoryMock = new();
    private readonly FollowUserUseCase _sut;

    public FollowUserUseCaseTest()
    {
        _sut = new FollowUserUseCase(_userFollowsRepositoryMock.Object);
    }

    [Fact]
    public async Task AddFollower_ShouldAddFollower()
    {
        Domain.UserFollowsAggregate.UserFollows userFollows = new(userId: "followerId");
        _userFollowsRepositoryMock
            .Setup(m => m.GetById(Domain.UserFollowsAggregate.UserFollows.BuildId(userFollows.UserId)))
            .ReturnsAsync(userFollows);

        await _sut.AddFollower("leaderId", userFollows.UserId, FixedDateTime);

        Assert.Equal("leaderId", userFollows.Follows[0].LeaderId);
    }

    [Fact]
    public async Task AddFollower_ShouldNotAddFollower_WhenAlreadyFollowed()
    {
        Domain.UserFollowsAggregate.UserFollows userFollows = new(userId: "followerId");
        userFollows.AddFollows("leaderId", FixedDateTime);
        _userFollowsRepositoryMock
            .Setup(m => m.GetById(Domain.UserFollowsAggregate.UserFollows.BuildId(userFollows.UserId)))
            .ReturnsAsync(userFollows);

        await _sut.AddFollower("leaderId", userFollows.UserId, FixedDateTime);

        Assert.Single(userFollows.Follows);
    }

    [Fact]
    public async Task RemoveFollower_ShouldRemoveFollower()
    {
        Domain.UserFollowsAggregate.UserFollows userFollows = new(userId: "followerId");
        userFollows.AddFollows("leaderId", FixedDateTime);
        _userFollowsRepositoryMock
            .Setup(m => m.GetById(Domain.UserFollowsAggregate.UserFollows.BuildId(userFollows.UserId)))
            .ReturnsAsync(userFollows);

        await _sut.RemoveFollower("leaderId", "followerId");

        Assert.DoesNotContain(userFollows.Follows, u => u.LeaderId == "leaderId");
    }
}
