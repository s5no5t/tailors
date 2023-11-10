using Moq;
using Tailors.Domain.ThreadAggregate;
using Tailors.Domain.TweedAggregate;
using Tailors.Domain.UserAggregate;
using Tailors.Domain.UserFollowsAggregate;

namespace Tailors.Domain.Test.ThreadAggregate;

public class ShowFeedUseCaseTest
{
    private static readonly DateTime FixedDateTime = new(2022, 11, 18, 15, 20, 0);

    private readonly ShowFeedUseCase _sut;
    private readonly TweedRepositoryMock _tweedRepositoryMock = new();

    public ShowFeedUseCaseTest()
    {
        Mock<IUserFollowsRepository> userFollowsRepositoryMock = new();
        userFollowsRepositoryMock.Setup(m => m.GetById(It.IsAny<string>()))
            .ReturnsAsync(new UserFollows("userId/Follows"));
        FollowUserUseCase followsUserUseCase = new(userFollowsRepositoryMock.Object);
        _sut = new ShowFeedUseCase(_tweedRepositoryMock, followsUserUseCase);
    }

    [Fact]
    public async Task GetFeed_ShouldReturnTweedsByCurrentUser()
    {
        Tweed currentUserTweed = new("userId", "test", FixedDateTime.AddHours(1));
        await _tweedRepositoryMock.Create(currentUserTweed);

        var tweeds = await _sut.GetFeed("userId", 0, 20);

        Assert.Contains(currentUserTweed, tweeds);
    }

    [Fact]
    public async Task GetFeed_ShouldReturnTweedsByFollowedUsers()
    {
        var followedUser = new AppUser();

        Tweed followedUserTweed = new(followedUser.Id!, "test", FixedDateTime.AddHours(1));
        await _tweedRepositoryMock.Create(followedUserTweed);

        var tweeds = await _sut.GetFeed("userId", 0, 20);

        Assert.Contains(followedUserTweed, tweeds);
    }

    [Fact]
    public async Task GetFeed_ShouldReturnPageSizeTweeds()
    {
        var ownTweeds = Enumerable.Range(0, 25).Select(i =>
        {
            Tweed tweed = new("userId", "test", FixedDateTime, $"tweeds/{i}");
            return tweed;
        }).ToList();
        foreach (var ownTweed in ownTweeds) await _tweedRepositoryMock.Create(ownTweed);

        var feed = await _sut.GetFeed("userId", 0, 20);

        Assert.Equal(20, feed.Count);
    }

    [Fact]
    public async Task GetFeed_ShouldRespectPage()
    {
        var ownTweeds = Enumerable.Range(0, 25).Select(i =>
        {
            Tweed tweed = new("userId", "test", FixedDateTime, $"tweeds/{i}");
            return tweed;
        }).ToList();
        foreach (var ownTweed in ownTweeds) await _tweedRepositoryMock.Create(ownTweed);

        var page0Feed = await _sut.GetFeed("userId", 0, 10);
        var page1Feed = await _sut.GetFeed("userId", 1, 10);

        Assert.NotEqual(page0Feed[0].Id, page1Feed[0].Id);
    }
}
