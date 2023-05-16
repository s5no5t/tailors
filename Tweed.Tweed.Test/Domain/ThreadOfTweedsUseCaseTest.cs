using FluentResults.Extensions.FluentAssertions;
using Moq;
using Tweed.Domain.Model;
using Xunit;

namespace Tweed.Domain.Test;

public class ThreadOfTweedsUseCaseTest
{
    private readonly ThreadOfTweedsUseCase _sut;
    private readonly Mock<ITweedRepository> _tweedRepositoryMock = new();
    private readonly Mock<ITweedThreadRepository> _tweedThreadRepositoryMock = new();

    public ThreadOfTweedsUseCaseTest()
    {
        _sut = new ThreadOfTweedsUseCase(_tweedThreadRepositoryMock.Object,
            _tweedRepositoryMock.Object);
    }

    [Fact]
    public async Task GetThreadTweedsForTweed_ShouldReturnFail_WhenTweedIsntFound()
    {
        var tweeds = await _sut.GetThreadTweedsForTweed("unknownTweedId");

        tweeds.Should().BeFailure();
    }

    [Fact]
    public async Task GetThreadTweedsForTweed_ShouldReturnRootTweed_WhenThereIsOnlyRoot()
    {
        TheTweed rootTheTweed = new()
        {
            Id = "rootTweedId"
        };
        TweedThread thread = new()
        {
            Id = "threadId",
            Root = new TweedThread.TweedReference
            {
                TweedId = rootTheTweed.Id
            }
        };
        _tweedThreadRepositoryMock.Setup(t => t.GetById(thread.Id)).ReturnsAsync(thread);
        _tweedRepositoryMock.Setup(m => m.GetById("rootTweedId")).ReturnsAsync(rootTheTweed);

        var tweeds = await _sut.GetThreadTweedsForTweed("rootTweedId");

        tweeds.Should().BeSuccess();
        Assert.Empty(tweeds.Value);
    }

    [Fact]
    public async Task GetThreadTweedsForTweed_ShouldReturnTweeds_WhenThereIsOneLeadingTweed()
    {
        TheTweed rootTheTweed = new()
        {
            Id = "rootTweedId"
        };
        TheTweed theTweed = new()
        {
            Id = "tweedId",
            ThreadId = "threadId"
        };
        _tweedRepositoryMock.Setup(m => m.GetById(theTweed.Id)).ReturnsAsync(theTweed);
        TweedThread thread = new()
        {
            Id = "threadId",
            Root = new TweedThread.TweedReference
            {
                TweedId = "rootTweedId",
                Replies = new List<TweedThread.TweedReference>
                {
                    new()
                    {
                        TweedId = "tweedId"
                    }
                }
            }
        };
        _tweedThreadRepositoryMock.Setup(t => t.GetById(thread.Id)).ReturnsAsync(thread);

        _tweedRepositoryMock.Setup(m =>
                m.GetByIds(CollectionMatcher(new[] { "rootTweedId", "tweedId" })))
            .ReturnsAsync(
                new Dictionary<string, TheTweed>
                {
                    { rootTheTweed.Id, rootTheTweed },
                    { theTweed.Id, theTweed }
                });

        var tweeds = await _sut.GetThreadTweedsForTweed("tweedId");

        tweeds.Should().BeSuccess();
        Assert.Equal("rootTweedId", tweeds.Value[0].Id);
        Assert.Equal("tweedId", tweeds.Value[1].Id);
    }

    public static IEnumerable<T> CollectionMatcher<T>(IEnumerable<T> expectation)
    {
        return Match.Create((IEnumerable<T> inputCollection) =>
            !expectation.Except(inputCollection).Any() &&
            !inputCollection.Except(expectation).Any());
    }

    [Fact]
    public async Task GetThreadTweedsForTweed_ShouldReturnTweeds_WhenThereIsAnotherBranch()
    {
        TheTweed rootTheTweed = new()
        {
            Id = "rootTweedId"
        };
        TheTweed parentTheTweed = new()
        {
            Id = "parentTweedId"
        };
        TheTweed theTweed = new()
        {
            Id = "tweedId",
            ThreadId = "threadId"
        };
        _tweedRepositoryMock.Setup(m => m.GetById(theTweed.Id)).ReturnsAsync(theTweed);
        TweedThread thread = new()
        {
            Id = "threadId",
            Root = new TweedThread.TweedReference
            {
                TweedId = "rootTweedId",
                Replies = new List<TweedThread.TweedReference>
                {
                    new()
                    {
                        TweedId = "parentTweedId",
                        Replies = new List<TweedThread.TweedReference>
                        {
                            new()
                            {
                                TweedId = "tweedId"
                            }
                        }
                    },
                    new()
                    {
                        TweedId = "otherTweedId"
                    }
                }
            }
        };
        _tweedThreadRepositoryMock.Setup(t => t.GetById(thread.Id)).ReturnsAsync(thread);
        _tweedRepositoryMock.Setup(m =>
                m.GetByIds(CollectionMatcher(new[] { "rootTweedId", "parentTweedId", "tweedId" })))
            .ReturnsAsync(
                new Dictionary<string, TheTweed>
                {
                    { rootTheTweed.Id, rootTheTweed },
                    { parentTheTweed.Id, parentTheTweed },
                    { theTweed.Id, theTweed }
                });

        var tweeds = await _sut.GetThreadTweedsForTweed("tweedId");

        tweeds.Should().BeSuccess();
        Assert.Equal("rootTweedId", tweeds.Value[0].Id);
        Assert.Equal("parentTweedId", tweeds.Value[1].Id);
        Assert.Equal("tweedId", tweeds.Value[2].Id);
    }

    [Fact]
    public async Task AddTweedToThread_ShouldSetRootTweed_IfParentTweedIdIsNull()
    {
        TheTweed theTweed = new()
        {
            Id = "tweedId",
            ThreadId = "threadId"
        };
        _tweedRepositoryMock.Setup(m => m.GetById(theTweed.Id)).ReturnsAsync(theTweed);
        TweedThread thread = new();
        _tweedThreadRepositoryMock.Setup(m => m.GetById("threadId")).ReturnsAsync(thread);

        var result = await _sut.AddTweedToThread("tweedId");

        result.Should().BeSuccess();
        Assert.Equal("tweedId", thread.Root.TweedId);
    }

    [Fact]
    public async Task AddTweedToThread_ShouldInsertReplyToRootTweed()
    {
        TheTweed theTweed = new()
        {
            Id = "tweedId",
            ParentTweedId = "rootTweedId",
            ThreadId = "threadId"
        };
        _tweedRepositoryMock.Setup(m => m.GetById(theTweed.Id)).ReturnsAsync(theTweed);
        TweedThread thread = new()
        {
            Id = "threadId",
            Root = new TweedThread.TweedReference
            {
                TweedId = "rootTweedId"
            }
        };
        _tweedThreadRepositoryMock.Setup(m => m.GetById("threadId")).ReturnsAsync(thread);

        var result = await _sut.AddTweedToThread("tweedId");

        result.Should().BeSuccess();
        Assert.Equal("tweedId", thread.Root.Replies[0].TweedId);
    }

    [Fact]
    public async Task AddTweedToThread_ShouldInsertReplyToReplyTweed()
    {
        TheTweed theTweed = new()
        {
            Id = "tweedId",
            ParentTweedId = "replyTweedId",
            ThreadId = "threadId"
        };
        _tweedRepositoryMock.Setup(m => m.GetById(theTweed.Id)).ReturnsAsync(theTweed);
        TweedThread thread = new()
        {
            Id = "threadId",
            Root = new TweedThread.TweedReference
            {
                TweedId = "rootTweedId",
                Replies = new List<TweedThread.TweedReference>
                {
                    new()
                    {
                        TweedId = "replyTweedId"
                    }
                }
            }
        };
        _tweedThreadRepositoryMock.Setup(m => m.GetById("threadId")).ReturnsAsync(thread);

        var result = await _sut.AddTweedToThread("tweedId");

        result.Should().BeSuccess();
        Assert.Equal("tweedId", thread.Root.Replies[0].Replies[0].TweedId);
    }
}