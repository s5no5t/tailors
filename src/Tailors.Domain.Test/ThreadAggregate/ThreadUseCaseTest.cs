using JetBrains.Annotations;
using OneOf.Types;
using Tailors.Domain.ThreadAggregate;
using Tailors.Domain.TweedAggregate;

namespace Tailors.Domain.Test.ThreadAggregate;

public class ThreadUseCaseTest
{
    private static readonly DateTime FixedDateTime = new(2022, 11, 18, 15, 20, 0);
    private readonly ThreadUseCase _sut;
    private readonly ThreadRepositoryMock _threadRepositoryMock = new();
    private readonly TweedRepositoryMock _tweedRepositoryMock = new();

    public ThreadUseCaseTest()
    {
        _sut = new ThreadUseCase(_threadRepositoryMock, _tweedRepositoryMock);
    }

    [Fact]
    public async Task GetThreadTweedsForTweed_ShouldReturnFail_WhenTweedNotFound()
    {
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
        await _tweedRepositoryMock.Create(rootTweed);
        TailorsThread thread = new("threadId");
        thread.AddTweed(rootTweed);
        await _threadRepositoryMock.Create(thread);

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
        await _tweedRepositoryMock.Create(rootTweed);
        Tweed tweed = new(id: "tweedId", parentTweedId: rootTweed.Id!, threadId: "threadId",
            authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        await _tweedRepositoryMock.Create(tweed);

        TailorsThread thread = new("threadId");
        thread.AddTweed(rootTweed);
        thread.AddTweed(tweed);
        await _threadRepositoryMock.Create(thread);

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
        await _tweedRepositoryMock.Create(rootTweed);
        Tweed parentTweed = new(id: "parentTweedId", parentTweedId: "rootTweedId", threadId: "threadId",
            authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        await _tweedRepositoryMock.Create(parentTweed);
        Tweed tweed = new(id: "tweedId", parentTweedId: "parentTweedId", threadId: "threadId",
            authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        await _tweedRepositoryMock.Create(tweed);
        Tweed otherTweed = new(id: "otherTweedId", parentTweedId: "rootTweedId", threadId: "threadId",
            authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);

        TailorsThread thread = new("threadId");
        thread.AddTweed(rootTweed);
        thread.AddTweed(parentTweed);
        thread.AddTweed(tweed);
        thread.AddTweed(otherTweed);
        await _threadRepositoryMock.Create(thread);

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
        var parentThread = await CreateThread(10);
        await _threadRepositoryMock.Create(parentThread);
        var childThread = new TailorsThread("childThreadId", parentThread.Id);
        await _threadRepositoryMock.Create(childThread);

        Tweed tweed = new(id: "tweedId", parentTweedId: "tweed-10", threadId: childThread.Id,
            authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        await _tweedRepositoryMock.Create(tweed);
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
        await _tweedRepositoryMock.Create(tweed);

        var result = await _sut.AddTweedToThread("tweedId");

        result.Switch(
            _ => { Assert.NotNull(tweed.ThreadId); },
            e => Assert.Fail(e.Message));
    }

    [Fact]
    public async Task AddTweedToThread_ShouldSetRootTweed_IfParentTweedIdIsNull()
    {
        Tweed tweed = new(id: "tweedId", authorId: "authorId", createdAt: FixedDateTime,
            text: string.Empty);
        await _tweedRepositoryMock.Create(tweed);

        var result = await _sut.AddTweedToThread("tweedId");

        Assert.IsType<Success>(result.Value);
        var thread = await _threadRepositoryMock.GetById(tweed.ThreadId!);
        Assert.Equal("tweedId", thread.AsT0.Root?.TweedId);
    }

    [Fact]
    public async Task AddTweedToThread_ShouldInsertReplyToRootTweed()
    {
        Tweed rootTweed = new(id: "rootTweedId", threadId: "threadId", authorId: "authorId",
            createdAt: FixedDateTime, text: string.Empty);
        await _tweedRepositoryMock.Create(rootTweed);
        Tweed tweed = new(id: "tweedId", parentTweedId: "rootTweedId",
            authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        await _tweedRepositoryMock.Create(tweed);
        TailorsThread thread = new("threadId");
        thread.AddTweed(rootTweed);
        await _threadRepositoryMock.Create(thread);

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
        await _tweedRepositoryMock.Create(rootTweed);
        Tweed replyTweed = new(id: "replyTweedId", parentTweedId: "rootTweedId", threadId: "threadId",
            authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        await _tweedRepositoryMock.Create(replyTweed);
        Tweed tweed = new(id: "tweedId", parentTweedId: "replyTweedId",
            authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        await _tweedRepositoryMock.Create(tweed);

        TailorsThread thread = new("threadId");
        thread.AddTweed(rootTweed);
        thread.AddTweed(replyTweed);
        await _threadRepositoryMock.Create(thread);

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
        await _tweedRepositoryMock.Create(childTweed);
        Tweed parentTweed = new(id: "parentTweedId", authorId: "authorId", createdAt: FixedDateTime,
            text: string.Empty, threadId: "threadId");
        await _tweedRepositoryMock.Create(parentTweed);
        TailorsThread thread = new("threadId");
        thread.AddTweed(parentTweed);
        await _threadRepositoryMock.Create(thread);

        var result = await _sut.AddTweedToThread("childTweedId");

        result.Switch(
            _ => { Assert.Equal("threadId", childTweed.ThreadId); },
            e => Assert.Fail(e.Message));
    }

    [Fact]
    public async Task AddTweedToThread_ShouldCreateSubThread_WhenParentTweedIsInThreadWhereMaxDepthReached()
    {
        var threadWithMaxDepthReached = await CreateThread(TailorsThread.MaxTweedReferenceDepth - 1);
        await _threadRepositoryMock.Create(threadWithMaxDepthReached);
        Tweed childTweed = new(id: "childTweedId", authorId: "authorId", text: "text", createdAt: FixedDateTime,
            parentTweedId: $"tweed-{TailorsThread.MaxTweedReferenceDepth - 1}");
        await _tweedRepositoryMock.Create(childTweed);

        var result = await _sut.AddTweedToThread("childTweedId");

        Assert.IsType<Success>(result.Value);
        var childThread = await _threadRepositoryMock.GetById(childTweed.ThreadId!);
        Assert.Equal(threadWithMaxDepthReached.Id, childThread.AsT0.ParentThreadId);
    }

    [Fact]
    public async Task AddTweedToThread_ShouldReturn_WhenTweedAlreadyBelongsToThread()
    {
        Tweed tweed = new(id: "tweedId", authorId: "authorId", createdAt: FixedDateTime, text: string.Empty,
            threadId: "threadId");
        await _tweedRepositoryMock.Create(tweed);

        var result = await _sut.AddTweedToThread("tweedId");

        result.Switch(
            _ => { Assert.Equal("threadId", tweed.ThreadId); },
            e => Assert.Fail(e.Message));
    }

    private async Task<TailorsThread> CreateThread(int depth)
    {
        TailorsThread threadWithMaxDepthReached = new("parentThreadId");

        Tweed rootTweed = new(id: "tweed-0", authorId: "authorId", createdAt: FixedDateTime,
            text: string.Empty, threadId: threadWithMaxDepthReached.Id);
        threadWithMaxDepthReached.AddTweed(rootTweed);
        await _tweedRepositoryMock.Create(rootTweed);

        for (var i = 1; i <= depth; i++)
        {
            Tweed tweed = new(id: $"tweed-{i}", authorId: "authorId", createdAt: FixedDateTime,
                text: string.Empty, parentTweedId: $"tweed-{i - 1}", threadId: threadWithMaxDepthReached.Id);
            threadWithMaxDepthReached.AddTweed(tweed);
            await _tweedRepositoryMock.Create(tweed);
        }

        return threadWithMaxDepthReached;
    }
}
