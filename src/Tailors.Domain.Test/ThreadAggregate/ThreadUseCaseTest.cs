using JetBrains.Annotations;
using Moq;
using OneOf.Types;
using Tailors.Domain.ThreadAggregate;
using Tailors.Domain.TweedAggregate;

namespace Tailors.Domain.Test.ThreadAggregate;

public class ThreadUseCaseTest
{
    private static readonly DateTime FixedDateTime = new(2022, 11, 18, 15, 20, 0);
    private readonly ThreadUseCase _sut;
    private readonly Mock<ITweedRepository> _tweedRepositoryMock = new();
    private readonly Mock<IThreadRepository> _threadRepositoryMock = new();

    public ThreadUseCaseTest()
    {
        _sut = new ThreadUseCase(_threadRepositoryMock.Object,
            _tweedRepositoryMock.Object);
    }

    [Fact]
    public async Task GetThreadTweedsForTweed_ShouldReturnFail_WhenTweedNotFound()
    {
        _tweedRepositoryMock.Setup(t => t.GetById("unknownTweedId")).ReturnsAsync(new None());

        var result = await _sut.GetThreadTweedsForTweed("unknownTweedId");

        result.Switch(
            _ => Assert.Fail("Should not return success"),
            _ => { });
    }

    [Fact]
    public async Task GetThreadTweedsForTweed_ShouldReturnRootTweed_WhenThereIsOnlyRoot()
    {
        Tweed rootTweed = new(id: "rootTweedId", authorId: "authorId", createdAt: FixedDateTime,
            text: string.Empty, threadId: "threadId");
        _tweedRepositoryMock.Setup(m => m.GetById("rootTweedId")).ReturnsAsync(rootTweed);
        TailorsThread thread = new("threadId");
        thread.AddTweed(rootTweed);
        _threadRepositoryMock.Setup(t => t.GetById(thread.Id!)).ReturnsAsync(thread);
        _tweedRepositoryMock.Setup(t => t.GetByIds(MoqExtensions.CollectionMatcher(new[] { "rootTweedId" })))
            .ReturnsAsync(new Dictionary<string, Tweed>
            {
                { rootTweed.Id!, rootTweed }
            });

        var result = await _sut.GetThreadTweedsForTweed("rootTweedId");

        result.Switch(
            t => Assert.Equal("rootTweedId", t[0].Id),
            e => Assert.Fail(e.Message));
    }

    [Fact]
    public async Task GetThreadTweedsForTweed_ShouldReturnTweeds_WhenThereIsOneLeadingTweed()
    {
        Tweed rootTweed = new(id: "rootTweedId", threadId: "threadId", authorId: "authorId",
            createdAt: FixedDateTime, text: string.Empty);
        Tweed tweed = new(id: "tweedId", parentTweedId: rootTweed.Id!, threadId: "threadId",
            authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        _tweedRepositoryMock.Setup(m => m.GetById(tweed.Id!)).ReturnsAsync(tweed);

        TailorsThread thread = new("threadId");
        thread.AddTweed(rootTweed);
        thread.AddTweed(tweed);

        _threadRepositoryMock.Setup(t => t.GetById(thread.Id!)).ReturnsAsync(thread);

        _tweedRepositoryMock.Setup(m =>
                m.GetByIds(MoqExtensions.CollectionMatcher(new[] { "rootTweedId", "tweedId" })))
            .ReturnsAsync(
                new Dictionary<string, Tweed>
                {
                    { rootTweed.Id!, rootTweed },
                    { tweed.Id!, tweed }
                });

        var result = await _sut.GetThreadTweedsForTweed("tweedId");

        result.Switch([AssertionMethod] (s) =>
            {
                Assert.Equal("rootTweedId", s[0].Id);
                Assert.Equal("tweedId", s[1].Id);
            },
            e => Assert.Fail(e.Message));
    }

    [Fact]
    public async Task GetThreadTweedsForTweed_ShouldReturnTweeds_WhenThereIsAnotherBranch()
    {
        Tweed rootTweed = new(id: "rootTweedId", threadId: "threadId", authorId: "authorId",
            createdAt: FixedDateTime, text: string.Empty);
        Tweed parentTweed = new(id: "parentTweedId", parentTweedId: "rootTweedId", threadId: "threadId",
            authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        Tweed tweed = new(id: "tweedId", parentTweedId: "parentTweedId", threadId: "threadId",
            authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        _tweedRepositoryMock.Setup(m => m.GetById(tweed.Id!)).ReturnsAsync(tweed);
        Tweed otherTweed = new(id: "otherTweedId", parentTweedId: "rootTweedId", threadId: "threadId",
            authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);

        TailorsThread thread = new("threadId");
        thread.AddTweed(rootTweed);
        thread.AddTweed(parentTweed);
        thread.AddTweed(tweed);
        thread.AddTweed(otherTweed);
        _threadRepositoryMock.Setup(t => t.GetById(thread.Id!)).ReturnsAsync(thread);
        _tweedRepositoryMock.Setup(m =>
                m.GetByIds(MoqExtensions.CollectionMatcher(new[] { "rootTweedId", "parentTweedId", "tweedId" })))
            .ReturnsAsync(
                new Dictionary<string, Tweed>
                {
                    { rootTweed.Id!, rootTweed },
                    { parentTweed.Id!, parentTweed },
                    { tweed.Id!, tweed }
                });

        var result = await _sut.GetThreadTweedsForTweed("tweedId");

        result.Switch(
            [AssertionMethod] (s) =>
            {
                Assert.Equal("rootTweedId", s[0].Id);
                Assert.Equal("parentTweedId", s[1].Id);
                Assert.Equal("tweedId", s[2].Id);
            },
            e => Assert.Fail(e.Message));
    }

    [Fact(Skip = "not implemented")]
    public async Task GetThreadTweedsForTweed_ShouldReturnTweeds_WhenThereIsASubThread()
    {
        var parentThread = CreateThread(10);
        _threadRepositoryMock.Setup(t => t.GetById(parentThread.Id!)).ReturnsAsync(parentThread);
        var childThread = new TailorsThread("childThreadId", parentThread.Id);
        _threadRepositoryMock.Setup(t => t.GetById(childThread.Id!)).ReturnsAsync(childThread);

        Tweed tweed = new(id: "tweedId", parentTweedId: "tweed-10", threadId: childThread.Id,
            authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        _tweedRepositoryMock.Setup(m => m.GetById(tweed.Id!)).ReturnsAsync(tweed);
        childThread.AddTweed(tweed);

        var result = await _sut.GetThreadTweedsForTweed("tweedId");

        result.Switch(
            [AssertionMethod] (tweeds) =>
            {
                Assert.Equal("rootTweedId", tweeds.First().Id);
                Assert.Equal("tweedId", tweeds.Last().Id);
            },
            e => Assert.Fail(e.Message));
    }


    [Fact]
    public async Task AddTweedToThread_ShouldCreateThread_WhenThreadIdIsNull()
    {
        Tweed tweed = new(id: "tweedId", authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        _tweedRepositoryMock.Setup(m => m.GetById(tweed.Id!)).ReturnsAsync(tweed);
        _threadRepositoryMock.Setup(t => t.Create(It.IsAny<TailorsThread>()))
            .Callback((TailorsThread t) => t.Id = "threadId").Returns(Task.CompletedTask);

        var result = await _sut.AddTweedToThread("tweedId");

        result.Switch(
            _ => { Assert.Equal("threadId", tweed.ThreadId); },
            e => Assert.Fail(e.Message));
    }

    [Fact]
    public async Task AddTweedToThread_ShouldSetRootTweed_IfParentTweedIdIsNull()
    {
        Tweed tweed = new(id: "tweedId", authorId: "authorId", createdAt: FixedDateTime,
            text: string.Empty);
        _tweedRepositoryMock.Setup(m => m.GetById(tweed.Id!)).ReturnsAsync(tweed);

        TailorsThread? thread = null;
        _threadRepositoryMock.Setup(t => t.Create(It.IsAny<TailorsThread>()))
            .Callback((TailorsThread t) =>
            {
                t.Id = "threadId";
                thread = t;
            }).Returns(Task.CompletedTask);

        var result = await _sut.AddTweedToThread("tweedId");

        result.Switch(
            _ => { Assert.Equal("tweedId", thread!.Root?.TweedId); },
            e => Assert.Fail(e.Message));
    }

    [Fact]
    public async Task AddTweedToThread_ShouldInsertReplyToRootTweed()
    {
        Tweed rootTweed = new(id: "rootTweedId", threadId: "threadId", authorId: "authorId",
            createdAt: FixedDateTime, text: string.Empty);
        _tweedRepositoryMock.Setup(m => m.GetById(rootTweed.Id!)).ReturnsAsync(rootTweed);
        Tweed tweed = new(id: "tweedId", parentTweedId: "rootTweedId",
            authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        _tweedRepositoryMock.Setup(m => m.GetById(tweed.Id!)).ReturnsAsync(tweed);
        TailorsThread thread = new("threadId");
        thread.AddTweed(rootTweed);
        _threadRepositoryMock.Setup(m => m.GetById("threadId")).ReturnsAsync(thread);

        var result = await _sut.AddTweedToThread("tweedId");

        result.Switch(
            _ => { Assert.Equal("tweedId", thread.Root?.Replies[0].TweedId); },
            e => Assert.Fail(e.Message));
    }

    [Fact]
    public async Task AddTweedToThread_ShouldInsertReplyToReplyTweed()
    {
        Tweed rootTweed = new(id: "rootTweedId", threadId: "threadId", authorId: "authorId",
            createdAt: FixedDateTime, text: string.Empty);
        _tweedRepositoryMock.Setup(m => m.GetById(rootTweed.Id!)).ReturnsAsync(rootTweed);
        Tweed replyTweed = new(id: "replyTweedId", parentTweedId: "rootTweedId", threadId: "threadId",
            authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        _tweedRepositoryMock.Setup(m => m.GetById(replyTweed.Id!)).ReturnsAsync(replyTweed);
        Tweed tweed = new(id: "tweedId", parentTweedId: "replyTweedId",
            authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        _tweedRepositoryMock.Setup(m => m.GetById(tweed.Id!)).ReturnsAsync(tweed);

        TailorsThread thread = new("threadId");
        thread.AddTweed(rootTweed);
        thread.AddTweed(replyTweed);
        _threadRepositoryMock.Setup(m => m.GetById("threadId")).ReturnsAsync(thread);

        var result = await _sut.AddTweedToThread("tweedId");

        result.Switch(
            _ => { Assert.Equal("tweedId", thread.Root?.Replies[0].Replies[0].TweedId); },
            e => Assert.Fail(e.Message));
    }

    [Fact]
    public async Task AddTweedToThread_ShouldInsertReplyIntoParentTweedThread()
    {
        Tweed childTweed = new(id: "childTweedId", authorId: "authorId", text: "text", createdAt: FixedDateTime,
            parentTweedId: "parentTweedId");
        _tweedRepositoryMock.Setup(t => t.GetById(childTweed.Id!)).ReturnsAsync(childTweed);
        Tweed parentTweed = new(id: "parentTweedId", authorId: "authorId", createdAt: FixedDateTime,
            text: string.Empty, threadId: "threadId");
        _tweedRepositoryMock.Setup(t => t.GetById(parentTweed.Id!)).ReturnsAsync(parentTweed);
        TailorsThread thread = new("threadId");
        thread.AddTweed(parentTweed);
        _threadRepositoryMock.Setup(t => t.GetById(thread.Id!)).ReturnsAsync(thread);

        var result = await _sut.AddTweedToThread("childTweedId");

        result.Switch(
            _ => { Assert.Equal("threadId", childTweed.ThreadId); },
            e => Assert.Fail(e.Message));
    }

    [Fact]
    public async Task AddTweedToThread_ShouldCreateSubThread_WhenParentTweedIsInThreadWhereMaxDepthReached()
    {
        var threadWithMaxDepthReached = CreateThread(TailorsThread.MaxTweedReferenceDepth - 1);
        _threadRepositoryMock.Setup(t => t.GetById(threadWithMaxDepthReached.Id!))
            .ReturnsAsync(threadWithMaxDepthReached);
        Tweed childTweed = new(id: "childTweedId", authorId: "authorId", text: "text", createdAt: FixedDateTime,
            parentTweedId: $"tweed-{TailorsThread.MaxTweedReferenceDepth - 1}");
        _tweedRepositoryMock.Setup(t => t.GetById(childTweed.Id!)).ReturnsAsync(childTweed);
        TailorsThread? childThread = null;
        _threadRepositoryMock.Setup(t => t.Create(It.IsAny<TailorsThread>()))
            .Callback((TailorsThread t) =>
            {
                t.Id = "childThreadId";
                childThread = t;
            }).Returns(Task.CompletedTask);

        var result = await _sut.AddTweedToThread("childTweedId");

        result.Switch(
            _ => { Assert.Equal(childThread!.Id, childTweed.ThreadId); },
            e => Assert.Fail(e.Message));
    }

    [Fact]
    public async Task AddTweedToThread_ShouldReturn_WhenTweedAlreadyBelongsToThread()
    {
        Tweed tweed = new(id: "tweedId", authorId: "authorId", createdAt: FixedDateTime, text: string.Empty, threadId: "threadId");
        _tweedRepositoryMock.Setup(m => m.GetById(tweed.Id!)).ReturnsAsync(tweed);

        var result = await _sut.AddTweedToThread("tweedId");

        result.Switch(
            _ => { Assert.Equal("threadId", tweed.ThreadId); },
            e => Assert.Fail(e.Message));
    }

    private TailorsThread CreateThread(int depth)
    {
        TailorsThread threadWithMaxDepthReached = new("parentThreadId");

        Tweed rootTweed = new(id: $"tweed-0", authorId: "authorId", createdAt: FixedDateTime,
            text: string.Empty, threadId: threadWithMaxDepthReached.Id);
        threadWithMaxDepthReached.AddTweed(rootTweed);
        _tweedRepositoryMock.Setup(t => t.GetById(rootTweed.Id!)).ReturnsAsync(rootTweed);

        for (var i = 1; i <= depth; i++)
        {
            Tweed tweed = new(id: $"tweed-{i}", authorId: "authorId", createdAt: FixedDateTime,
                text: string.Empty, parentTweedId: $"tweed-{i - 1}", threadId: threadWithMaxDepthReached.Id);
            threadWithMaxDepthReached.AddTweed(tweed);
            _tweedRepositoryMock.Setup(t => t.GetById(tweed.Id!)).ReturnsAsync(tweed);
        }

        return threadWithMaxDepthReached;
    }
}
