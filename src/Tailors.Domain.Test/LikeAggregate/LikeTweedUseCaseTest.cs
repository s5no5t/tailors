using OneOf;
using OneOf.Types;
using Tailors.Domain.UserLikesAggregate;

namespace Tailors.Domain.Test.LikeAggregate;

public class UserLikesRepositoryMock : IUserLikesRepository
{
    private readonly Dictionary<string, long> _likesCounter = new();
    private readonly Dictionary<string, UserLikes> _userLikes = new();

    public Task<OneOf<UserLikes, None>> GetById(string userLikesId)
    {
        _userLikes.TryGetValue(userLikesId, out var userLikes);

        if (userLikes is not null)
            return Task.FromResult<OneOf<UserLikes, None>>(userLikes);

        return Task.FromResult<OneOf<UserLikes, None>>(new None());
    }

    public Task Create(UserLikes userLikes)
    {
        userLikes.Id = UserLikes.BuildId(userLikes.UserId);

        _userLikes.Add(userLikes.Id!, userLikes);
        return Task.CompletedTask;
    }

    public Task<long> GetLikesCounter(string tweedId)
    {
        _likesCounter.TryGetValue(tweedId, out var likesCounter);

        return Task.FromResult(likesCounter);
    }

    public void IncreaseLikesCounter(string tweedId)
    {
        _likesCounter.TryGetValue(tweedId, out var likesCounter);
        _likesCounter[tweedId] = likesCounter + 1;
    }

    public void DecreaseLikesCounter(string tweedId)
    {
        _likesCounter.TryGetValue(tweedId, out var likesCounter);
        _likesCounter[tweedId] = likesCounter - 1;
    }
}

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
