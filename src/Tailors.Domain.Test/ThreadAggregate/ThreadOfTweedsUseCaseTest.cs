using JetBrains.Annotations;
using Moq;
using Tailors.Domain.ThreadAggregate;
using Tailors.Domain.TweedAggregate;

namespace Tailors.Domain.Test.ThreadAggregate;

public class ThreadOfTweedsUseCaseTest
{
    private static readonly DateTime FixedDateTime = new DateTime(2022, 11, 18, 15, 20, 0);
    private readonly ThreadOfTweedsUseCase _sut;
    private readonly Mock<ITweedRepository> _tweedRepositoryMock = new();
    private readonly Mock<IThreadRepository> _tweedThreadRepositoryMock = new();

    public ThreadOfTweedsUseCaseTest()
    {
        _sut = new ThreadOfTweedsUseCase(_tweedThreadRepositoryMock.Object,
            _tweedRepositoryMock.Object);
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
            text: string.Empty);
        _tweedRepositoryMock.Setup(m => m.GetById("rootTweedId")).ReturnsAsync(rootTweed);
        TailorsThread thread = new(id: "threadId");
        thread.AddTweed(rootTweed);
        _tweedThreadRepositoryMock.Setup(t => t.GetById(thread.Id!)).ReturnsAsync(thread);

        var result = await _sut.GetThreadTweedsForTweed("rootTweedId");

        result.Switch(
            Assert.Empty,
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

        TailorsThread thread = new(id: "threadId");
        thread.AddTweed(rootTweed);
        thread.AddTweed(tweed);

        _tweedThreadRepositoryMock.Setup(t => t.GetById(thread.Id!)).ReturnsAsync(thread);

        _tweedRepositoryMock.Setup(m =>
                m.GetByIds(MoqExtensions.CollectionMatcher(new[] { "rootTweedId", "tweedId" })))
            .ReturnsAsync(
                new Dictionary<string, Tweed>
                {
                    { rootTweed.Id!, rootTweed },
                    { tweed.Id!, tweed }
                });

        var result = await _sut.GetThreadTweedsForTweed("tweedId");

        result.Switch([AssertionMethod](s) =>
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

        TailorsThread thread = new(id: "threadId");
        thread.AddTweed(rootTweed);
        thread.AddTweed(parentTweed);
        thread.AddTweed(tweed);
        thread.AddTweed(otherTweed);
        _tweedThreadRepositoryMock.Setup(t => t.GetById(thread.Id!)).ReturnsAsync(thread);
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
            [AssertionMethod](s) =>
            {
                Assert.Equal("rootTweedId", s[0].Id);
                Assert.Equal("parentTweedId", s[1].Id);
                Assert.Equal("tweedId", s[2].Id);
            },
            e => Assert.Fail(e.Message));
    }

    [Fact]
    public async Task AddTweedToThread_ShouldCreateThread_WhenThreadIdIsNull()
    {
        Tweed tweed = new(id: "tweedId", authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        _tweedRepositoryMock.Setup(m => m.GetById(tweed.Id!)).ReturnsAsync(tweed);
        _tweedThreadRepositoryMock.Setup(t => t.Create(It.IsAny<TailorsThread>()))
            .Callback((TailorsThread t) => t.Id = "threadId").Returns(Task.CompletedTask);

        var result = await _sut.AddTweedToThread("tweedId");

        result.Switch(
            _ => { Assert.Equal("threadId", tweed.ThreadId); },
            e => Assert.Fail(e.Message),
            e => Assert.Fail(e.Message));
    }

    [Fact]
    public async Task AddTweedToThread_ShouldSetRootTweed_IfParentTweedIdIsNull()
    {
        Tweed tweed = new(id: "tweedId", threadId: "threadId", authorId: "authorId", createdAt: FixedDateTime,
            text: string.Empty);
        _tweedRepositoryMock.Setup(m => m.GetById(tweed.Id!)).ReturnsAsync(tweed);
        TailorsThread thread = new();
        _tweedThreadRepositoryMock.Setup(m => m.GetById("threadId")).ReturnsAsync(thread);

        var result = await _sut.AddTweedToThread("tweedId");

        result.Switch(
            _ => { Assert.Equal("tweedId", thread.Root?.TweedId); },
            e => Assert.Fail(e.Message),
            e => Assert.Fail(e.Message));
    }

    [Fact]
    public async Task AddTweedToThread_ShouldInsertReplyToRootTweed()
    {
        Tweed rootTweed = new(id: "rootTweedId", threadId: "threadId", authorId: "authorId",
            createdAt: FixedDateTime, text: string.Empty);
        _tweedRepositoryMock.Setup(m => m.GetById(rootTweed.Id!)).ReturnsAsync(rootTweed);
        Tweed tweed = new(id: "tweedId", parentTweedId: "rootTweedId", threadId: "threadId",
            authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        _tweedRepositoryMock.Setup(m => m.GetById(tweed.Id!)).ReturnsAsync(tweed);
        TailorsThread thread = new(id: "threadId");
        thread.AddTweed(rootTweed);

        _tweedThreadRepositoryMock.Setup(m => m.GetById("threadId")).ReturnsAsync(thread);

        var result = await _sut.AddTweedToThread("tweedId");

        result.Switch(
            _ => { Assert.Equal("tweedId", thread.Root?.Replies[0].TweedId); },
            e => Assert.Fail(e.Message),
            e => Assert.Fail(e.Message));
    }

    [Fact]
    public async Task AddTweedToThread_ShouldInsertReplyToReplyTweed()
    {
        Tweed rootTweed = new(id: "rootTweedId", threadId: "threadId", authorId: "authorId",
            createdAt: FixedDateTime, text: string.Empty);
        Tweed replyTweed = new(id: "replyTweedId", parentTweedId: "rootTweedId", threadId: "threadId",
            authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        Tweed tweed = new(id: "tweedId", parentTweedId: "replyTweedId", threadId: "threadId",
            authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        _tweedRepositoryMock.Setup(m => m.GetById(tweed.Id!)).ReturnsAsync(tweed);

        TailorsThread thread = new(id: "threadId");
        thread.AddTweed(rootTweed);
        thread.AddTweed(replyTweed);

        _tweedThreadRepositoryMock.Setup(m => m.GetById("threadId")).ReturnsAsync(thread);

        var result = await _sut.AddTweedToThread("tweedId");

        result.Switch(
            _ => { Assert.Equal("tweedId", thread.Root?.Replies[0].Replies[0].TweedId); },
            e => Assert.Fail(e.Message),
            e => Assert.Fail(e.Message));
    }

    [Fact(Skip = "not implemented")]
    public async Task AddTweedToThread_ShouldCreateSubThread_WhenParentTweedIsInThreadWhereMaxDepthReached()
    {
        Tweed tweed = new(id: "childTweedId", authorId: "authorId", text: "text", createdAt: FixedDateTime,
            parentTweedId: "parentTweedId");
        _tweedRepositoryMock.Setup(t => t.GetById("childTweedId")).ReturnsAsync(tweed);

        var result = await _sut.AddTweedToThread("childTweedId");

        result.Switch(
            _ => { Assert.Equal("childThreadId", tweed.ThreadId); },
            e => Assert.Fail(e.Message),
            e => Assert.Fail(e.Message));
    }
}