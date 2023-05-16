using Moq;
using NodaTime;
using Tweed.Domain;
using Tweed.Domain.Model;
using Tweed.Like.Domain;
using Xunit;

namespace Tweed.Like.Test.Domain;

public class LikeTweedUseCaseTest
{
    private static readonly ZonedDateTime FixedZonedDateTime =
        new(new LocalDateTime(2022, 11, 18, 15, 20), DateTimeZone.Utc, new Offset());

    private readonly LikeTweedUseCase _sut;

    private readonly Mock<ITweedLikesRepository> _tweedLikesRepositoryMock = new();

    public LikeTweedUseCaseTest()
    {
        _sut = new LikeTweedUseCase(_tweedLikesRepositoryMock.Object);
    }

    [Fact]
    public async Task AddLike_ShouldIncreaseLikes()
    {
        var userLikes = new UserLikes
        {
            UserId = "userId"
        };
        _tweedLikesRepositoryMock.Setup(m => m.GetById("userId/Likes")).ReturnsAsync(userLikes);

        await _sut.AddLike("tweedId", "userId", FixedZonedDateTime);

        Assert.Single(userLikes.Likes);
    }

    [Fact]
    public async Task AddLike_ShouldIncreaseLikesCounter()
    {
        var tweed = new Tweed.Domain.Model.Tweed
        {
            Id = "tweedId"
        };

        await _sut.AddLike("tweedId", "userId", FixedZonedDateTime);

        _tweedLikesRepositoryMock.Verify(t => t.IncreaseLikesCounter(tweed.Id), Times.Once);
    }

    [Fact]
    public async Task AddLike_ShouldNotIncreaseLikes_WhenUserHasAlreadyLiked()
    {
        var userLikes = new UserLikes
        {
            UserId = "userId",
            Likes = new List<UserLikes.TweedLike>
            {
                new()
                {
                    TweedId = "tweedId"
                }
            }
        };
        _tweedLikesRepositoryMock.Setup(m => m.GetById("userId")).ReturnsAsync(userLikes);

        await _sut.AddLike("tweedId", "userId", FixedZonedDateTime);

        Assert.Single(userLikes.Likes);
    }

    [Fact]
    public async Task RemoveLike_ShouldDecreaseLikes()
    {
        var userLikes = new UserLikes
        {
            UserId = "userId",
            Likes = new List<UserLikes.TweedLike>
            {
                new()
                {
                    TweedId = "tweedId"
                }
            }
        };
        _tweedLikesRepositoryMock.Setup(m => m.GetById("userId/Likes")).ReturnsAsync(userLikes);

        await _sut.RemoveLike("tweedId", "userId");

        Assert.Empty(userLikes.Likes);
    }

    [Fact]
    public async Task RemoveLike_ShouldDecreaseLikesCounter()
    {
        var userLikes = new UserLikes
        {
            UserId = "userId"
        };
        _tweedLikesRepositoryMock.Setup(m => m.GetById("userId")).ReturnsAsync(userLikes);

        await _sut.RemoveLike("tweedId", "userId");

        _tweedLikesRepositoryMock.Verify(t => t.DecreaseLikesCounter("tweedId"), Times.Once);
    }

    [Fact]
    public async Task RemoveLike_ShouldNotDecreaseLikes_WhenUserAlreadyDoesntLike()
    {
        var userLikes = new UserLikes
        {
            UserId = "userId"
        };
        _tweedLikesRepositoryMock.Setup(m => m.GetById("userId")).ReturnsAsync(userLikes);

        await _sut.RemoveLike("tweedId", "userId");

        Assert.Empty(userLikes.Likes);
    }
}
