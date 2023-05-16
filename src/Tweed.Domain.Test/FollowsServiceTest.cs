using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NodaTime;
using Tweed.Domain.Model;
using Xunit;

namespace Tweed.Domain.Test;

public class FollowsServiceTest
{
    private static readonly ZonedDateTime FixedZonedDateTime =
        new(new LocalDateTime(2022, 11, 18, 15, 20), DateTimeZone.Utc, new Offset());

    private readonly Mock<IUserFollowsRepository> _userFollowsRepositoryMock = new();
    private readonly FollowsService _sut;

    public FollowsServiceTest()
    {
        _sut = new FollowsService(_userFollowsRepositoryMock.Object);
    }

    [Fact]
    public async Task AddFollower_ShouldAddFollower()
    {
        UserFollows userFollows = new()
        {
            UserId = "followerId"
        };
        _userFollowsRepositoryMock
            .Setup(m => m.GetById(UserFollows.BuildId(userFollows.UserId)))
            .ReturnsAsync(userFollows);

        await _sut.AddFollower("leaderId", userFollows.UserId, FixedZonedDateTime);

        Assert.Equal("leaderId", userFollows.Follows[0].LeaderId);
    }

    [Fact]
    public async Task AddFollower_ShouldNotAddFollower_WhenAlreadyFollowed()
    {
        UserFollows userFollows = new()
        {
            UserId = "followerId",
            Follows = new List<UserFollows.LeaderReference>
            {
                new()
                {
                    LeaderId = "leaderId"
                }
            }
        };
        _userFollowsRepositoryMock
            .Setup(m => m.GetById(UserFollows.BuildId(userFollows.UserId)))
            .ReturnsAsync(userFollows);

        await _sut.AddFollower("leaderId", userFollows.UserId, FixedZonedDateTime);

        Assert.Single(userFollows.Follows);
    }

    [Fact]
    public async Task RemoveFollower_ShouldRemoveFollower()
    {
        UserFollows userFollows = new()
        {
            UserId = "followerId",
            Follows = new List<UserFollows.LeaderReference>
            {
                new()
                {
                    LeaderId = "leaderId"
                }
            }
        };
        _userFollowsRepositoryMock
            .Setup(m => m.GetById(UserFollows.BuildId(userFollows.UserId)))
            .ReturnsAsync(userFollows);

        await _sut.RemoveFollower("leaderId", "followerId");

        Assert.DoesNotContain(userFollows.Follows, u => u.LeaderId == "leaderId");
    }
}
