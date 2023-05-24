using Moq;
using Tailors.Thread.Domain;
using Xunit;

namespace Tailors.Thread.Test.Domain;

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
        
        Assert.True(tweeds.IsT1);
    }

    [Fact]
    public async Task GetThreadTweedsForTweed_ShouldReturnRootTweed_WhenThereIsOnlyRoot()
    {
        Tweed rootTweed = new()
        {
            Id = "rootTweedId"
        };
        TweedThread thread = new()
        {
            Id = "threadId",
            Root = new TweedThread.TweedReference
            {
                TweedId = rootTweed.Id
            }
        };
        _tweedThreadRepositoryMock.Setup(t => t.GetById(thread.Id)).ReturnsAsync(thread);
        _tweedRepositoryMock.Setup(m => m.GetById("rootTweedId")).ReturnsAsync(rootTweed);

        var tweeds = await _sut.GetThreadTweedsForTweed("rootTweedId");

        Assert.True(tweeds.IsT0);
        Assert.Empty(tweeds.AsT0);
    }

    [Fact]
    public async Task GetThreadTweedsForTweed_ShouldReturnTweeds_WhenThereIsOneLeadingTweed()
    {
        Tweed rootTweed = new()
        {
            Id = "rootTweedId"
        };
        Tweed tweed = new()
        {
            Id = "tweedId",
            ThreadId = "threadId"
        };
        _tweedRepositoryMock.Setup(m => m.GetById(tweed.Id)).ReturnsAsync(tweed);
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
                m.GetByIds(MoqExtensions.CollectionMatcher(new[] { "rootTweedId", "tweedId" })))
            .ReturnsAsync(
                new Dictionary<string, Tweed>
                {
                    { rootTweed.Id, rootTweed },
                    { tweed.Id, tweed }
                });

        var tweeds = await _sut.GetThreadTweedsForTweed("tweedId");

        Assert.True(tweeds.IsT0);
        Assert.Equal("rootTweedId", tweeds.AsT0[0].Id);
        Assert.Equal("tweedId", tweeds.AsT0[1].Id);
    }

    [Fact]
    public async Task GetThreadTweedsForTweed_ShouldReturnTweeds_WhenThereIsAnotherBranch()
    {
        Tweed rootTweed = new()
        {
            Id = "rootTweedId"
        };
        Tweed parentTweed = new()
        {
            Id = "parentTweedId"
        };
        Tweed tweed = new()
        {
            Id = "tweedId",
            ThreadId = "threadId"
        };
        _tweedRepositoryMock.Setup(m => m.GetById(tweed.Id)).ReturnsAsync(tweed);
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
                m.GetByIds(MoqExtensions.CollectionMatcher(new[] { "rootTweedId", "parentTweedId", "tweedId" })))
            .ReturnsAsync(
                new Dictionary<string, Tweed>
                {
                    { rootTweed.Id, rootTweed },
                    { parentTweed.Id, parentTweed },
                    { tweed.Id, tweed }
                });

        var tweeds = await _sut.GetThreadTweedsForTweed("tweedId");

        Assert.True(tweeds.IsT0);
        Assert.Equal("rootTweedId", tweeds.AsT0[0].Id);
        Assert.Equal("parentTweedId", tweeds.AsT0[1].Id);
        Assert.Equal("tweedId", tweeds.AsT0[2].Id);
    }

    [Fact]
    public async Task AddTweedToThread_ShouldSetRootTweed_IfParentTweedIdIsNull()
    {
        Tweed tweed = new()
        {
            Id = "tweedId",
            ThreadId = "threadId"
        };
        _tweedRepositoryMock.Setup(m => m.GetById(tweed.Id)).ReturnsAsync(tweed);
        TweedThread thread = new();
        _tweedThreadRepositoryMock.Setup(m => m.GetById("threadId")).ReturnsAsync(thread);

        var result = await _sut.AddTweedToThread("tweedId");

        Assert.True(result.IsT0);
        Assert.Equal("tweedId", thread.Root.TweedId);
    }

    [Fact]
    public async Task AddTweedToThread_ShouldInsertReplyToRootTweed()
    {
        Tweed tweed = new()
        {
            Id = "tweedId",
            ParentTweedId = "rootTweedId",
            ThreadId = "threadId"
        };
        _tweedRepositoryMock.Setup(m => m.GetById(tweed.Id)).ReturnsAsync(tweed);
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

        Assert.True(result.IsT0);
        Assert.Equal("tweedId", thread.Root.Replies[0].TweedId);
    }

    [Fact]
    public async Task AddTweedToThread_ShouldInsertReplyToReplyTweed()
    {
        Tweed tweed = new()
        {
            Id = "tweedId",
            ParentTweedId = "replyTweedId",
            ThreadId = "threadId"
        };
        _tweedRepositoryMock.Setup(m => m.GetById(tweed.Id)).ReturnsAsync(tweed);
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

        Assert.True(result.IsT0);
        Assert.Equal("tweedId", thread.Root.Replies[0].Replies[0].TweedId);
    }
}
