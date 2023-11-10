using Tailors.Domain.UserLikesAggregate;

namespace Tailors.Domain.Test.LikeAggregate;

public class LikeTweedUseCaseTest
{
    private static readonly DateTime FixedDateTime = new(2022, 11, 18, 15, 20, 0);

    private readonly LikeTweedUseCase _sut;

    private readonly UserLikesRepositoryMock _userLikesRepositoryMock = new();

    public LikeTweedUseCaseTest()
    {
        _sut = new LikeTweedUseCase(_userLikesRepositoryMock);
    }

    [Fact]
    public async Task AddLike_ShouldIncreaseLikes()
    {
        await _sut.AddLike("tweedId", "userId", FixedDateTime);

        var userLikes = await _userLikesRepositoryMock.GetById("userId/Likes");
        Assert.Single(userLikes.AsT0.Likes);
    }

    [Fact]
    public async Task AddLike_ShouldIncreaseLikesCounter()
    {
        await _sut.AddLike("tweedId", "userId", FixedDateTime);

        var counter = await _userLikesRepositoryMock.GetLikesCounter("tweedId");
        Assert.Equal(1, counter);
    }

    [Fact]
    public async Task AddLike_ShouldNotIncreaseLikes_WhenUserHasAlreadyLiked()
    {
        var userLikes = new UserLikes("userId");
        userLikes.AddLike("tweedId", FixedDateTime);
        await _userLikesRepositoryMock.Create(userLikes);

        await _sut.AddLike("tweedId", "userId", FixedDateTime);

        Assert.Single(userLikes.Likes);
    }

    [Fact]
    public async Task RemoveLike_ShouldDecreaseLikes()
    {
        var userLikes = new UserLikes("userId");
        userLikes.AddLike("tweedId", FixedDateTime);
        await _userLikesRepositoryMock.Create(userLikes);

        await _sut.RemoveLike("tweedId", "userId");

        Assert.Empty(userLikes.Likes);
    }

    [Fact]
    public async Task RemoveLike_ShouldDecreaseLikesCounter()
    {
        var userLikes = new UserLikes("userId");
        userLikes.AddLike("tweedId", FixedDateTime);
        await _userLikesRepositoryMock.Create(userLikes);

        await _sut.RemoveLike("tweedId", "userId");

        Assert.Empty(userLikes.Likes);
    }

    [Fact]
    public async Task RemoveLike_ShouldNotDecreaseLikes_WhenUserAlreadyDoesntLike()
    {
        var userLikes = new UserLikes("userId");
        await _userLikesRepositoryMock.Create(userLikes);

        await _sut.RemoveLike("tweedId", "userId");

        Assert.Empty(userLikes.Likes);
    }
}
