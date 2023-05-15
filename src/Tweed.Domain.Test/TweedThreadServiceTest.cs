using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Tweed.Domain.Model;
using Xunit;

namespace Tweed.Domain.Test;

public class TweedThreadServiceTest
{
    private readonly ThreadService _sut;
    private readonly Mock<ITweedThreadRepository> _tweedThreadRepositoryMock = new();

    public TweedThreadServiceTest()
    {
        _sut = new ThreadService(_tweedThreadRepositoryMock.Object);
    }

    [Fact]
    public async Task GetLeadingTweeds_ShouldReturnNull_WhenTweedIsntFound()
    {
        TweedThread thread = new()
        {
            Id = "threadId",
            Root = new TweedThread.TweedReference
            {
                TweedId = "rootTweedId"
            }
        };
        _tweedThreadRepositoryMock.Setup(t => t.GetById(thread.Id)).ReturnsAsync(thread);

        var leadingTweeds = await _sut.GetLeadingTweeds("threadId", "unknownTweedId");

        Assert.Null(leadingTweeds);
    }

    [Fact]
    public async Task GetLeadingTweeds_ShouldReturnEmptyList_WhenThereIsOnlyRoot()
    {
        TweedThread thread = new()
        {
            Id = "threadId",
            Root = new TweedThread.TweedReference
            {
                TweedId = "rootTweedId"
            }
        };
        _tweedThreadRepositoryMock.Setup(t => t.GetById(thread.Id)).ReturnsAsync(thread);

        var leadingTweeds = await _sut.GetLeadingTweeds(thread.Id, "rootTweedId");

        Assert.NotNull(leadingTweeds);
        Assert.Empty(leadingTweeds);
    }

    [Fact]
    public async Task GetLeadingTweeds_ShouldReturnLeadingTweed_WhenThereIsOneLeadingTweed()
    {
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

        var leadingTweeds = await _sut.GetLeadingTweeds("threadId", "tweedId");

        Assert.NotNull(leadingTweeds);
        Assert.Equal("rootTweedId", leadingTweeds[0].TweedId);
    }

    [Fact]
    public async Task GetLeadingTweeds_ShouldReturnLeadingTweeds_WhenThereIsAnotherBranch()
    {
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

        var leadingTweeds = await _sut.GetLeadingTweeds("threadId", "tweedId");

        Assert.NotNull(leadingTweeds);
        Assert.Equal("rootTweedId", leadingTweeds[0].TweedId);
        Assert.Equal("parentTweedId", leadingTweeds[1].TweedId);
    }

    [Fact]
    public async Task AddTweedToThread_ShouldSetRootTweed_IfParentTweedIdIsNull()
    {
        TweedThread thread = new();
        _tweedThreadRepositoryMock.Setup(m => m.GetById("threadId")).ReturnsAsync(thread);

        await _sut.AddTweedToThread("threadId", "tweedId", null);

        Assert.Equal("tweedId", thread.Root.TweedId);
    }

    [Fact]
    public async Task AddTweedToThread_ShouldInsertReplyToRootTweed()
    {
        TweedThread thread = new()
        {
            Id = "threadId",
            Root = new TweedThread.TweedReference
            {
                TweedId = "rootTweedId"
            }
        };
        _tweedThreadRepositoryMock.Setup(m => m.GetById("threadId")).ReturnsAsync(thread);

        await _sut.AddTweedToThread("threadId", "tweedId", "rootTweedId");

        Assert.Equal("tweedId", thread.Root.Replies[0].TweedId);
    }

    [Fact]
    public async Task AddTweedToThread_ShouldInsertReplyToReplyTweed()
    {
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

        await _sut.AddTweedToThread("threadId", "tweedId", "replyTweedId");

        Assert.Equal("tweedId", thread.Root.Replies[0].Replies[0].TweedId);
    }
}
