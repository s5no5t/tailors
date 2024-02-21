using JetBrains.Annotations;
using OneOf.Types;
using Tailors.Domain.ThreadAggregate;
using Tailors.Domain.TweedAggregate;

namespace Tailors.Domain.Test.ThreadAggregate;

public class ThreadUseCaseTest
{
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
        Tweed rootTweed = new(id: "rootTweedId", authorId: "authorId", createdAt: TestData.FixedDateTime,
            text: string.Empty, threadId: "threadId");
        await _tweedRepositoryMock.Create(rootTweed);

        var result = await _sut.GetThreadTweedsForTweed("rootTweedId");

        result.Switch(
            t => Assert.Equal("rootTweedId", t[0].Id),
            e => Assert.Fail(e.Message));
    }

    [Fact]
    public async Task GetThreadTweedsForTweed_ShouldReturnTweeds_WhenThereIsOneLeadingTweed()
    {
        Tweed rootTweed = new(id: "rootTweedId", threadId: "threadId", authorId: "authorId",
            createdAt: TestData.FixedDateTime, text: string.Empty);
        await _tweedRepositoryMock.Create(rootTweed);
        Tweed tweed = new(id: "tweedId", parentTweedId: rootTweed.Id!, threadId: "threadId",
            authorId: "authorId", createdAt: TestData.FixedDateTime, text: string.Empty);
        tweed.AddLeadingTweedId(rootTweed.Id!);
        await _tweedRepositoryMock.Create(tweed);

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
            createdAt: TestData.FixedDateTime, text: string.Empty);
        await _tweedRepositoryMock.Create(rootTweed);
        Tweed parentTweed = new(id: "parentTweedId", parentTweedId: "rootTweedId", threadId: "threadId",
            authorId: "authorId", createdAt: TestData.FixedDateTime, text: string.Empty);
        parentTweed.AddLeadingTweedId(rootTweed.Id!);
        await _tweedRepositoryMock.Create(parentTweed);
        Tweed tweed = new(id: "tweedId", parentTweedId: "parentTweedId", threadId: "threadId",
            authorId: "authorId", createdAt: TestData.FixedDateTime, text: string.Empty);
        tweed.AddLeadingTweedId(rootTweed.Id!);
        tweed.AddLeadingTweedId(parentTweed.Id!);
        await _tweedRepositoryMock.Create(tweed);
        Tweed otherTweed = new(id: "otherTweedId", parentTweedId: "rootTweedId", threadId: "threadId",
            authorId: "authorId", createdAt: TestData.FixedDateTime, text: string.Empty);
        otherTweed.AddLeadingTweedId(rootTweed.Id!);

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
            authorId: "authorId", createdAt: TestData.FixedDateTime, text: string.Empty);
        await _tweedRepositoryMock.Create(tweed);
        childThread.AddTweed(tweed.Id!, tweed.ParentTweedId);

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
        Tweed tweed = new(id: "tweedId", authorId: "authorId", createdAt: TestData.FixedDateTime, text: string.Empty);
        await _tweedRepositoryMock.Create(tweed);

        var result = await _sut.AddTweedToThread("tweedId");

        result.Switch(
            _ => { Assert.NotNull(tweed.ThreadId); },
            e => Assert.Fail(e.Message));
    }

    [Fact]
    public async Task AddTweedToThread_ShouldSetRootTweed_IfParentTweedIdIsNull()
    {
        Tweed tweed = new(id: "tweedId", authorId: "authorId", createdAt: TestData.FixedDateTime,
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
            createdAt: TestData.FixedDateTime, text: string.Empty);
        await _tweedRepositoryMock.Create(rootTweed);
        Tweed tweed = new(id: "tweedId", parentTweedId: "rootTweedId",
            authorId: "authorId", createdAt: TestData.FixedDateTime, text: string.Empty);
        await _tweedRepositoryMock.Create(tweed);
        TailorsThread thread = new("threadId");
        thread.AddTweed(rootTweed.Id!);
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
            createdAt: TestData.FixedDateTime, text: string.Empty);
        await _tweedRepositoryMock.Create(rootTweed);
        Tweed replyTweed = new(id: "replyTweedId", parentTweedId: "rootTweedId", threadId: "threadId",
            authorId: "authorId", createdAt: TestData.FixedDateTime, text: string.Empty);
        await _tweedRepositoryMock.Create(replyTweed);
        Tweed tweed = new(id: "tweedId", parentTweedId: "replyTweedId",
            authorId: "authorId", createdAt: TestData.FixedDateTime, text: string.Empty);
        await _tweedRepositoryMock.Create(tweed);

        TailorsThread thread = new("threadId");
        thread.AddTweed(rootTweed.Id!);
        thread.AddTweed(replyTweed.Id!, replyTweed.ParentTweedId);
        await _threadRepositoryMock.Create(thread);

        var result = await _sut.AddTweedToThread("tweedId");

        result.Switch(
            _ => { Assert.Equal("tweedId", thread.Root?.Replies[0].Replies[0].TweedId); },
            e => Assert.Fail(e.Message));
    }

    [Fact]
    public async Task AddTweedToThread_ShouldInsertReplyIntoParentTweedThread()
    {
        Tweed childTweed = new(id: "childTweedId", authorId: "authorId", text: "text",
            createdAt: TestData.FixedDateTime,
            parentTweedId: "parentTweedId");
        await _tweedRepositoryMock.Create(childTweed);
        Tweed parentTweed = new(id: "parentTweedId", authorId: "authorId", createdAt: TestData.FixedDateTime,
            text: string.Empty, threadId: "threadId");
        await _tweedRepositoryMock.Create(parentTweed);
        TailorsThread thread = new("threadId");
        thread.AddTweed(parentTweed.Id!);
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
        Tweed childTweed = new(id: "childTweedId", authorId: "authorId", text: "text",
            createdAt: TestData.FixedDateTime,
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
        Tweed tweed = new(id: "tweedId", authorId: "authorId", createdAt: TestData.FixedDateTime, text: string.Empty,
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

        Tweed rootTweed = new(id: "tweed-0", authorId: "authorId", createdAt: TestData.FixedDateTime,
            text: string.Empty, threadId: threadWithMaxDepthReached.Id);
        threadWithMaxDepthReached.AddTweed(rootTweed.Id!);
        await _tweedRepositoryMock.Create(rootTweed);

        for (var i = 1; i <= depth; i++)
        {
            Tweed tweed = new(id: $"tweed-{i}", authorId: "authorId", createdAt: TestData.FixedDateTime,
                text: string.Empty, parentTweedId: $"tweed-{i - 1}", threadId: threadWithMaxDepthReached.Id);
            threadWithMaxDepthReached.AddTweed(tweed.Id!, tweed.ParentTweedId);
            await _tweedRepositoryMock.Create(tweed);
        }

        return threadWithMaxDepthReached;
    }
}
