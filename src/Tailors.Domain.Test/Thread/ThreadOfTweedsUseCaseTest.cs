using Moq;
using Tailors.Domain.Thread;
using Tailors.Domain.Tweed;

namespace Tailors.Domain.Test.Thread;

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
        var tweeds = await _sut.GetThreadTweedsForTweed("unknownTweedId");

        Assert.True(tweeds.IsT1);
    }

    [Fact]
    public async Task GetThreadTweedsForTweed_ShouldReturnRootTweed_WhenThereIsOnlyRoot()
    {
        TailorsTweed rootTweed = new(id: "rootTweedId", authorId: "authorId", createdAt: FixedDateTime,
            text: string.Empty);
        _tweedRepositoryMock.Setup(m => m.GetById("rootTweedId")).ReturnsAsync(rootTweed);
        TailorsThread thread = new(id: "threadId");
        thread.AddTweed(rootTweed);
        _tweedThreadRepositoryMock.Setup(t => t.GetById(thread.Id!)).ReturnsAsync(thread);

        var tweeds = await _sut.GetThreadTweedsForTweed("rootTweedId");

        Assert.True(tweeds.IsT0);
        Assert.Empty(tweeds.AsT0);
    }

    [Fact]
    public async Task GetThreadTweedsForTweed_ShouldReturnTweeds_WhenThereIsOneLeadingTweed()
    {
        TailorsTweed rootTweed = new(id: "rootTweedId", threadId: "threadId", authorId: "authorId",
            createdAt: FixedDateTime, text: string.Empty);
        TailorsTweed tweed = new(id: "tweedId", parentTweedId: rootTweed.Id!, threadId: "threadId",
            authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        _tweedRepositoryMock.Setup(m => m.GetById(tweed.Id!)).ReturnsAsync(tweed);

        TailorsThread thread = new(id: "threadId");
        thread.AddTweed(rootTweed);
        thread.AddTweed(tweed);

        _tweedThreadRepositoryMock.Setup(t => t.GetById(thread.Id!)).ReturnsAsync(thread);

        _tweedRepositoryMock.Setup(m =>
                m.GetByIds(MoqExtensions.CollectionMatcher(new[] { "rootTweedId", "tweedId" })))
            .ReturnsAsync(
                new Dictionary<string, TailorsTweed>
                {
                    { rootTweed.Id!, rootTweed },
                    { tweed.Id!, tweed }
                });

        var tweeds = await _sut.GetThreadTweedsForTweed("tweedId");

        Assert.True(tweeds.IsT0);
        Assert.Equal("rootTweedId", tweeds.AsT0[0].Id);
        Assert.Equal("tweedId", tweeds.AsT0[1].Id);
    }

    [Fact]
    public async Task GetThreadTweedsForTweed_ShouldReturnTweeds_WhenThereIsAnotherBranch()
    {
        TailorsTweed rootTweed = new(id: "rootTweedId", threadId: "threadId", authorId: "authorId",
            createdAt: FixedDateTime, text: string.Empty);
        TailorsTweed parentTweed = new(id: "parentTweedId", parentTweedId: "rootTweedId", threadId: "threadId",
            authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        TailorsTweed tweed = new(id: "tweedId", parentTweedId: "parentTweedId", threadId: "threadId",
            authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        _tweedRepositoryMock.Setup(m => m.GetById(tweed.Id!)).ReturnsAsync(tweed);
        TailorsTweed otherTweed = new(id: "otherTweedId", parentTweedId: "rootTweedId", threadId: "threadId",
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
                new Dictionary<string, TailorsTweed>
                {
                    { rootTweed.Id!, rootTweed },
                    { parentTweed.Id!, parentTweed },
                    { tweed.Id!, tweed }
                });

        var tweeds = await _sut.GetThreadTweedsForTweed("tweedId");

        Assert.True(tweeds.IsT0);
        Assert.Equal("rootTweedId", tweeds.AsT0[0].Id);
        Assert.Equal("parentTweedId", tweeds.AsT0[1].Id);
        Assert.Equal("tweedId", tweeds.AsT0[2].Id);
    }

    [Fact]
    public async Task AddTweedToThread_ShouldCreateThread_WhenThreadIdIsNull()
    {
        TailorsTweed tweed = new(id: "tweedId", authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        _tweedRepositoryMock.Setup(m => m.GetById(tweed.Id!)).ReturnsAsync(tweed);
        _tweedThreadRepositoryMock.Setup(t => t.Create(It.IsAny<TailorsThread>()))
            .Callback((TailorsThread t) => t.Id = "threadId").Returns(Task.CompletedTask);

        var result = await _sut.AddTweedToThread("tweedId");

        Assert.True(result.IsT0);
        Assert.Equal("threadId", tweed.ThreadId);
    }

    [Fact]
    public async Task AddTweedToThread_ShouldSetRootTweed_IfParentTweedIdIsNull()
    {
        TailorsTweed tweed = new(id: "tweedId", threadId: "threadId", authorId: "authorId", createdAt: FixedDateTime,
            text: string.Empty);
        _tweedRepositoryMock.Setup(m => m.GetById(tweed.Id!)).ReturnsAsync(tweed);
        TailorsThread thread = new();
        _tweedThreadRepositoryMock.Setup(m => m.GetById("threadId")).ReturnsAsync(thread);

        var result = await _sut.AddTweedToThread("tweedId");

        Assert.True(result.IsT0);
        Assert.Equal("tweedId", thread.Root?.TweedId);
    }

    [Fact]
    public async Task AddTweedToThread_ShouldInsertReplyToRootTweed()
    {
        TailorsTweed rootTweed = new(id: "rootTweedId", threadId: "threadId", authorId: "authorId",
            createdAt: FixedDateTime, text: string.Empty);
        _tweedRepositoryMock.Setup(m => m.GetById(rootTweed.Id!)).ReturnsAsync(rootTweed);
        TailorsTweed tweed = new(id: "tweedId", parentTweedId: "rootTweedId", threadId: "threadId",
            authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        _tweedRepositoryMock.Setup(m => m.GetById(tweed.Id!)).ReturnsAsync(tweed);
        TailorsThread thread = new(id: "threadId");
        thread.AddTweed(rootTweed);

        _tweedThreadRepositoryMock.Setup(m => m.GetById("threadId")).ReturnsAsync(thread);

        var result = await _sut.AddTweedToThread("tweedId");

        Assert.True(result.IsT0);
        Assert.Equal("tweedId", thread.Root?.Replies[0].TweedId);
    }

    [Fact]
    public async Task AddTweedToThread_ShouldInsertReplyToReplyTweed()
    {
        TailorsTweed rootTweed = new(id: "rootTweedId", threadId: "threadId", authorId: "authorId",
            createdAt: FixedDateTime, text: string.Empty);
        TailorsTweed replyTweed = new(id: "replyTweedId", parentTweedId: "rootTweedId", threadId: "threadId",
            authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        TailorsTweed tweed = new(id: "tweedId", parentTweedId: "replyTweedId", threadId: "threadId",
            authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        _tweedRepositoryMock.Setup(m => m.GetById(tweed.Id!)).ReturnsAsync(tweed);

        TailorsThread thread = new(id: "threadId");
        thread.AddTweed(rootTweed);
        thread.AddTweed(replyTweed);

        _tweedThreadRepositoryMock.Setup(m => m.GetById("threadId")).ReturnsAsync(thread);

        var result = await _sut.AddTweedToThread("tweedId");

        Assert.True(result.IsT0);
        Assert.Equal("tweedId", thread.Root?.Replies[0].Replies[0].TweedId);
    }
}