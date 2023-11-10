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
    private readonly Mock<ITweedRepository> _tweedRepositoryMock = new();

    public ShowFeedUseCaseTest()
    {
        Mock<FollowUserUseCase> followsServiceMock = new(new Mock<IUserFollowsRepository>().Object);
        followsServiceMock.Setup(m => m.GetFollows(It.IsAny<string>()))
            .ReturnsAsync(new List<UserFollows.LeaderReference>());
        _tweedRepositoryMock.Setup(m => m.GetAllByAuthorId(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Tweed>());
        _tweedRepositoryMock
            .Setup(m => m.GetFollowerTweeds(It.IsAny<List<string>>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Tweed>());
        _tweedRepositoryMock
            .Setup(m => m.GetRecentTweeds(It.IsAny<int>()))
            .ReturnsAsync(new List<Tweed>());
        _sut = new ShowFeedUseCase(_tweedRepositoryMock.Object, followsServiceMock.Object);
    }

    [Fact]
    public async Task GetFeed_ShouldReturnTweedsByCurrentUser()
    {
        Tweed currentUserTweed = new("userId", "test", FixedDateTime.AddHours(1));
        _tweedRepositoryMock
            .Setup(m => m.GetAllByAuthorId(currentUserTweed.AuthorId, It.IsAny<int>()))
            .ReturnsAsync(new List<Tweed> { currentUserTweed });

        var tweeds = await _sut.GetFeed("userId", 0, 20);

        Assert.Contains(currentUserTweed, tweeds);
    }

    [Fact]
    public async Task GetFeed_ShouldReturnTweedsByFollowedUsers()
    {
        var followedUser = new AppUser();

        Tweed followedUserTweed = new(followedUser.Id!, "test", FixedDateTime.AddHours(1));
        _tweedRepositoryMock
            .Setup(m => m.GetFollowerTweeds(It.IsAny<List<string>>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Tweed> { followedUserTweed });

        var tweeds = await _sut.GetFeed("userId", 0, 20);

        Assert.Contains(followedUserTweed, tweeds);
    }

    [Fact]
    public async Task GetFeed_ShouldNotReturnTheSameTweedTwice()
    {
        Tweed currentUserTweed = new("userId", "test", FixedDateTime.AddHours(1));
        _tweedRepositoryMock
            .Setup(m => m.GetAllByAuthorId(currentUserTweed.AuthorId, It.IsAny<int>()))
            .ReturnsAsync(new List<Tweed> { currentUserTweed, currentUserTweed });

        var tweeds = await _sut.GetFeed("userId", 0, 20);

        var anyDuplicateTweed = tweeds.GroupBy(x => x.Id).Any(g => g.Count() > 1);
        Assert.False(anyDuplicateTweed);
    }

    [Fact]
    public async Task GetFeed_ShouldReturnPageSizeTweeds()
    {
        var ownTweeds = Enumerable.Range(0, 25).Select(i =>
        {
            Tweed tweed = new("userId", "test", FixedDateTime, $"tweeds/{i}");
            return tweed;
        }).ToList();
        _tweedRepositoryMock
            .Setup(m => m.GetAllByAuthorId("userId", It.IsAny<int>()))
            .ReturnsAsync(ownTweeds);

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
        _tweedRepositoryMock
            .Setup(m => m.GetAllByAuthorId("userId", It.IsAny<int>()))
            .ReturnsAsync(ownTweeds);

        var page0Feed = await _sut.GetFeed("userId", 0, 10);
        var page1Feed = await _sut.GetFeed("userId", 1, 10);

        Assert.NotEqual(page0Feed[0].Id, page1Feed[0].Id);
    }
}
