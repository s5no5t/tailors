using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NodaTime;
using Tweed.Domain.Model;
using Xunit;

namespace Tweed.Domain.Test;

public class ShowFeedUseCaseTest
{
    private static readonly ZonedDateTime FixedZonedDateTime =
        new(new LocalDateTime(2022, 11, 18, 15, 20), DateTimeZone.Utc, new Offset());

    private readonly Mock<IFollowUserUseCase> _followsServiceMock = new();
    private readonly ShowShowFeedUseCase _sut;
    private readonly Mock<ITweedRepository> _tweedRepositoryMock = new();

    public ShowFeedUseCaseTest()
    {
        _followsServiceMock.Setup(m => m.GetFollows(It.IsAny<string>()))
            .ReturnsAsync(new List<UserFollows.LeaderReference>());
        _tweedRepositoryMock.Setup(m => m.GetAllByAuthorId(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Domain.Model.Tweed>());
        _tweedRepositoryMock
            .Setup(m => m.GetFollowerTweeds(It.IsAny<List<string>>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Domain.Model.Tweed>());
        _tweedRepositoryMock
            .Setup(m => m.GetRecentTweeds(It.IsAny<int>()))
            .ReturnsAsync(new List<Domain.Model.Tweed>());
        _sut = new ShowShowFeedUseCase(_tweedRepositoryMock.Object, _followsServiceMock.Object);
    }

    [Fact]
    public async Task GetFeed_ShouldReturnTweedsByCurrentUser()
    {
        Domain.Model.Tweed currentUserTweed = new()
        {
            Text = "test",
            AuthorId = "userId",
            CreatedAt = FixedZonedDateTime.PlusHours(1)
        };
        _tweedRepositoryMock
            .Setup(m => m.GetAllByAuthorId(currentUserTweed.AuthorId, It.IsAny<int>()))
            .ReturnsAsync(new List<Domain.Model.Tweed> { currentUserTweed });

        var tweeds = await _sut.GetFeed("userId", 0, 20);

        Assert.Contains(currentUserTweed, tweeds);
    }

    [Fact]
    public async Task GetFeed_ShouldReturnTweedsByFollowedUsers()
    {
        var followedUser = new User();

        Domain.Model.Tweed followedUserTweed = new()
        {
            Text = "test",
            AuthorId = followedUser.Id!,
            CreatedAt = FixedZonedDateTime.PlusHours(1)
        };
        _tweedRepositoryMock
            .Setup(m => m.GetFollowerTweeds(It.IsAny<List<string>>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Domain.Model.Tweed> { followedUserTweed });

        var tweeds = await _sut.GetFeed("userId", 0, 20);

        Assert.Contains(followedUserTweed, tweeds);
    }

    [Fact]
    public async Task GetFeed_ShouldNotReturnTheSameTweedTwice()
    {
        Domain.Model.Tweed currentUserTweed = new()
        {
            Text = "test",
            AuthorId = "userId",
            CreatedAt = FixedZonedDateTime.PlusHours(1)
        };
        _tweedRepositoryMock
            .Setup(m => m.GetAllByAuthorId(currentUserTweed.AuthorId, It.IsAny<int>()))
            .ReturnsAsync(new List<Domain.Model.Tweed> { currentUserTweed, currentUserTweed });

        var tweeds = await _sut.GetFeed("userId", 0, 20);

        var anyDuplicateTweed = tweeds.GroupBy(x => x.Id).Any(g => g.Count() > 1);
        Assert.False(anyDuplicateTweed);
    }

    [Fact]
    public async Task GetFeed_ShouldReturnPageSizeTweeds()
    {
        var ownTweeds = Enumerable.Range(0, 25).Select(i =>
        {
            Domain.Model.Tweed tweed = new()
            {
                Id = $"tweeds/{i}",
                Text = "test",
                AuthorId = "userId",
                CreatedAt = FixedZonedDateTime
            };
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
            Domain.Model.Tweed tweed = new()
            {
                Id = $"tweeds/{i}",
                Text = "test",
                AuthorId = "userId",
                CreatedAt = FixedZonedDateTime
            };
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
