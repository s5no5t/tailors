using JetBrains.Annotations;
using Tailors.Domain.TweedAggregate;

namespace Tailors.Domain.Test.TweedAggregate;

public class ThreadUseCaseTest
{
    private readonly ThreadUseCase _sut;
    private readonly TweedRepositoryMock _tweedRepositoryMock = new();

    public ThreadUseCaseTest()
    {
        _sut = new ThreadUseCase(_tweedRepositoryMock);
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
        Tweed rootTweed = new(id: "rootTweedId", authorId: "authorId", createdAt: TestData.FixedDateTime, text: string.Empty);
        await _tweedRepositoryMock.Create(rootTweed);

        var result = await _sut.GetThreadTweedsForTweed("rootTweedId");

        result.Switch(
            t => Assert.Equal("rootTweedId", t[0].Id),
            e => Assert.Fail(e.Message));
    }

    [Fact]
    public async Task GetThreadTweedsForTweed_ShouldReturnTweeds_WhenThereIsOneLeadingTweed()
    {
        Tweed rootTweed = new(id: "rootTweedId", authorId: "authorId", createdAt: TestData.FixedDateTime, text: string.Empty);
        await _tweedRepositoryMock.Create(rootTweed);
        Tweed tweed = new(id: "tweedId", authorId: "authorId", createdAt: TestData.FixedDateTime, text: string.Empty);
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
        Tweed rootTweed = new(id: "rootTweedId", authorId: "authorId", createdAt: TestData.FixedDateTime, text: string.Empty);
        await _tweedRepositoryMock.Create(rootTweed);
        Tweed parentTweed = new(id: "parentTweedId", authorId: "authorId", createdAt: TestData.FixedDateTime, text: string.Empty);
        parentTweed.AddLeadingTweedId(rootTweed.Id!);
        await _tweedRepositoryMock.Create(parentTweed);
        Tweed tweed = new(id: "tweedId", authorId: "authorId", createdAt: TestData.FixedDateTime, text: string.Empty);
        tweed.AddLeadingTweedId(rootTweed.Id!);
        tweed.AddLeadingTweedId(parentTweed.Id!);
        await _tweedRepositoryMock.Create(tweed);
        Tweed otherTweed = new(id: "otherTweedId", authorId: "authorId", createdAt: TestData.FixedDateTime, text: string.Empty);
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

    [Fact]
    public async Task GetReplyTweedsForTweed_ShouldReturnFail_WhenThereIsNoTweed()
    {
        var result = await _sut.GetReplyTweedsForTweed("unknownTweedId");

        result.Switch(
            _ => Assert.Fail("Should not return success"),
            _ => { });
    }

    [Fact]
    public async Task GetReplyTweedsForTweed_ShouldReturnReplyTweed()
    {
        Tweed tweed = new(id: "tweedId", authorId: "authorId", createdAt: TestData.FixedDateTime, text: string.Empty);
        await _tweedRepositoryMock.Create(tweed);
        Tweed replyTweed = new(id: "replyTweedId", authorId: "authorId", createdAt: TestData.FixedDateTime, text: string.Empty);
        replyTweed.AddLeadingTweedId(tweed.Id!);
        await _tweedRepositoryMock.Create(replyTweed);

        var result = await _sut.GetReplyTweedsForTweed("tweedId");

        result.Switch(
            t => Assert.Equal("replyTweedId", t[0].Id),
            e => Assert.Fail(e.Message));
    }
}
