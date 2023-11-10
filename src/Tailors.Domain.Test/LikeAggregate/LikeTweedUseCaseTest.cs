using Moq;
using Tailors.Domain.TweedAggregate;
using Tailors.Domain.UserLikesAggregate;

namespace Tailors.Domain.Test.LikeAggregate;

public class LikeTweedUseCaseTest
{
    private static readonly DateTime FixedDateTime = new(2022, 11, 18, 15, 20, 0);

    private readonly LikeTweedUseCase _sut;

    private readonly Mock<IUserLikesRepository> _tweedLikesRepositoryMock = new();

    public LikeTweedUseCaseTest()
    {
        _sut = new LikeTweedUseCase(_tweedLikesRepositoryMock.Object);
    }

    [Fact]
    public async Task AddLike_ShouldIncreaseLikes()
    {
        var userLikes = new UserLikes("userId");
        _tweedLikesRepositoryMock.Setup(m => m.GetById(UserLikes.BuildId("userId")))
            .ReturnsAsync(userLikes);

        await _sut.AddLike("tweedId", "userId", FixedDateTime);

        Assert.Single(userLikes.Likes);
    }

    [Fact]
    public async Task AddLike_ShouldIncreaseLikesCounter()
    {
        var tweed = new Tweed(id: "tweedId", text: string.Empty, authorId: "authorId", createdAt: FixedDateTime);
        _tweedLikesRepositoryMock.Setup(m => m.GetById(UserLikes.BuildId("userId")))
            .ReturnsAsync(new UserLikes("userId"));

        await _sut.AddLike("tweedId", "userId", FixedDateTime);

        _tweedLikesRepositoryMock.Verify(t => t.IncreaseLikesCounter(tweed.Id!), Times.Once);
    }

    [Fact]
    public async Task AddLike_ShouldNotIncreaseLikes_WhenUserHasAlreadyLiked()
    {
        var userLikes = new UserLikes("userId");
        userLikes.AddLike("tweedId", FixedDateTime);
        _tweedLikesRepositoryMock.Setup(m => m.GetById(UserLikes.BuildId("userId")))
            .ReturnsAsync(userLikes);

        await _sut.AddLike("tweedId", "userId", FixedDateTime);

        Assert.Single(userLikes.Likes);
        _tweedLikesRepositoryMock.Verify(m => m.IncreaseLikesCounter(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task RemoveLike_ShouldDecreaseLikes()
    {
        var userLikes = new UserLikes("userId");
        userLikes.AddLike("tweedId", FixedDateTime);
        _tweedLikesRepositoryMock.Setup(m => m.GetById(UserLikes.BuildId("userId")))
            .ReturnsAsync(userLikes);

        await _sut.RemoveLike("tweedId", "userId");

        Assert.Empty(userLikes.Likes);
    }

    [Fact]
    public async Task RemoveLike_ShouldDecreaseLikesCounter()
    {
        var userLikes = new UserLikes("userId");
        userLikes.AddLike("tweedId", FixedDateTime);
        _tweedLikesRepositoryMock.Setup(m => m.GetById(UserLikes.BuildId("userId")))
            .ReturnsAsync(userLikes);

        await _sut.RemoveLike("tweedId", "userId");

        _tweedLikesRepositoryMock.Verify(t => t.DecreaseLikesCounter("tweedId"), Times.Once);
    }

    [Fact]
    public async Task RemoveLike_ShouldNotDecreaseLikes_WhenUserAlreadyDoesntLike()
    {
        var userLikes = new UserLikes("userId");
        _tweedLikesRepositoryMock.Setup(m => m.GetById(UserLikes.BuildId("userId")))
            .ReturnsAsync(userLikes);

        await _sut.RemoveLike("tweedId", "userId");

        Assert.Empty(userLikes.Likes);
        _tweedLikesRepositoryMock.Verify(m => m.DecreaseLikesCounter(It.IsAny<string>()),
            Times.Never);
    }
}