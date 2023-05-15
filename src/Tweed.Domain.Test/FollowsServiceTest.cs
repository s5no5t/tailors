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

    private readonly Mock<IAppUserFollowsRepository> _appUserFollowsRepositoryMock = new();
    private readonly FollowsService _sut;

    public FollowsServiceTest()
    {
        _sut = new FollowsService(_appUserFollowsRepositoryMock.Object);
    }

    [Fact]
    public async Task AddFollower_ShouldAddFollower()
    {
        AppUserFollows appUserFollows = new()
        {
            AppUserId = "followerId"
        };
        _appUserFollowsRepositoryMock
            .Setup(m => m.GetById(AppUserFollows.BuildId(appUserFollows.AppUserId)))
            .ReturnsAsync(appUserFollows);

        await _sut.AddFollower("leaderId", appUserFollows.AppUserId, FixedZonedDateTime);

        Assert.Equal("leaderId", appUserFollows.Follows[0].LeaderId);
    }

    [Fact]
    public async Task AddFollower_ShouldNotAddFollower_WhenAlreadyFollowed()
    {
        AppUserFollows appUserFollows = new()
        {
            AppUserId = "followerId",
            Follows = new List<AppUserFollows.LeaderReference>
            {
                new()
                {
                    LeaderId = "leaderId"
                }
            }
        };
        _appUserFollowsRepositoryMock
            .Setup(m => m.GetById(AppUserFollows.BuildId(appUserFollows.AppUserId)))
            .ReturnsAsync(appUserFollows);

        await _sut.AddFollower("leaderId", appUserFollows.AppUserId, FixedZonedDateTime);

        Assert.Single(appUserFollows.Follows);
    }

    [Fact]
    public async Task RemoveFollower_ShouldRemoveFollower()
    {
        AppUserFollows appUserFollows = new()
        {
            AppUserId = "followerId",
            Follows = new List<AppUserFollows.LeaderReference>
            {
                new()
                {
                    LeaderId = "leaderId"
                }
            }
        };
        _appUserFollowsRepositoryMock
            .Setup(m => m.GetById(AppUserFollows.BuildId(appUserFollows.AppUserId)))
            .ReturnsAsync(appUserFollows);

        await _sut.RemoveFollower("leaderId", "followerId");

        Assert.DoesNotContain(appUserFollows.Follows, u => u.LeaderId == "leaderId");
    }
}
