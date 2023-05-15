using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NodaTime;
using Tweed.Domain.Model;
using Xunit;

namespace Tweed.Domain.Test;

public class FeedServiceTest
{
    private static readonly ZonedDateTime FixedZonedDateTime =
        new(new LocalDateTime(2022, 11, 18, 15, 20), DateTimeZone.Utc, new Offset());

    private readonly Mock<IFollowsService> _followsServiceMock = new();
    private readonly FeedService _sut;
    private readonly Mock<ITweedRepository> _tweedRepositoryMock = new();

    public FeedServiceTest()
    {
        _followsServiceMock.Setup(m => m.GetFollows(It.IsAny<string>()))
            .ReturnsAsync(new List<AppUserFollows.LeaderReference>());
        _tweedRepositoryMock.Setup(m => m.GetAllByAuthorId(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Domain.Model.Tweed>());
        _tweedRepositoryMock
            .Setup(m => m.GetFollowerTweeds(It.IsAny<List<string>>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Domain.Model.Tweed>());
        _tweedRepositoryMock
            .Setup(m => m.GetRecentTweeds(It.IsAny<int>()))
            .ReturnsAsync(new List<Domain.Model.Tweed>());
        _sut = new FeedService(_tweedRepositoryMock.Object, _followsServiceMock.Object);
    }

    /*public async Task InitializeAsync()
    {
        await using var bulkInsert = _store.BulkInsert();

        for (var i = 0; i < 5; i++)
        {
            Domain.Model.Tweed currentUserTweed = new()
            {
                Text = "test",
                AuthorId = _currentUser.Id!,
                CreatedAt = FixedZonedDateTime.PlusHours(-100)
            };
            await bulkInsert.StoreAsync(currentUserTweed);
        }

        List<AppUser> otherUsers = new();
        for (var i = 0; i < 5; i++)
        {
            AppUser otherUser = new();
            await bulkInsert.StoreAsync(otherUser);
            otherUsers.Add(otherUser);

            for (var j = 0; j < 20; j++)
            {
                Domain.Model.Tweed otherUserTweed = new()
                {
                    Text = "test",
                    AuthorId = otherUser.Id!,
                    CreatedAt = FixedZonedDateTime.PlusHours(-100)
                };
                await bulkInsert.StoreAsync(otherUserTweed);
            }
        }

        await bulkInsert.StoreAsync(_currentUser);

        AppUserFollows currentUserFollows = new()
        {
            AppUserId = _currentUser.Id,
            Follows = otherUsers.Take(3).Select(u => new AppUserFollows.LeaderReference
            {
                LeaderId = u.Id,
                CreatedAt = FixedZonedDateTime.PlusHours(-100)
            }).ToList()
        };
        await bulkInsert.StoreAsync(currentUserFollows);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }*/

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
        var followedUser = new AppUser();

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

    /*[Fact]
    public async Task GetFeed_ShouldReturn20TweedsPerPage()
    {
        var ownTweeds = Enumerable.Repeat(0, 25).Select(_ =>
        {
            Domain.Model.Tweed tweed = new()
            {
                Text = "test",
                AuthorId = _currentUser.Id!,
                CreatedAt = FixedZonedDateTime
            };
            return tweed;
        }).ToList();

        _tweedRepositoryMock.Setup(t => t.GetTweedsForAuthorId("userId", 20)).ReturnsAsync(ownTweeds);

        var feed = await _sut.GetFeed(_currentUser.Id!, 0);

        Assert.Equal(20, feed.Count);
    }*/

    // [Fact]
    // public async Task GetFeed_ShouldRespectPage()
    // {
    //     var page0Feed = await _sut.GetFeed(_currentUser.Id!, 0);
    //     var page1Feed = await _sut.GetFeed(_currentUser.Id!, 1);
    //
    //     Assert.NotEqual(page0Feed[0].Id, page1Feed[1].Id);
    // }
}
