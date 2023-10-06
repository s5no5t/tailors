using JetBrains.Annotations;
using Moq;
using Tailors.Domain.TweedAggregate;

namespace Tailors.Domain.Test.TweedAggregate;

public class CreateTweedUseCaseTest
{
    private static readonly DateTime FixedDateTime = new(2022, 11, 18, 15, 20, 0);

    private readonly CreateTweedUseCase _sut;
    private readonly Mock<ITweedRepository> _tweedRepositoryMock = new();

    public CreateTweedUseCaseTest()
    {
        _sut = new CreateTweedUseCase(_tweedRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateRootTweed_SavesTweed()
    {
        var result = await _sut.CreateRootTweed("authorId", "text", FixedDateTime);

        result.Switch(
            [AssertionMethod](t) => { _tweedRepositoryMock.Verify(s => s.Create(It.IsAny<Tweed>())); }
        );
    }

    [Fact]
    public async Task CreateReplyTweed_SavesTweed()
    {
        Tweed parentTweed = new(id: "parentTweedId", threadId: "threadId", authorId: "authorId",
            createdAt: FixedDateTime, text: string.Empty);
        _tweedRepositoryMock.Setup(t => t.GetById(parentTweed.Id!)).ReturnsAsync(parentTweed);

        var result = await _sut.CreateReplyTweed("authorId", "text", FixedDateTime, parentTweed.Id!);

        result.Switch(
            [AssertionMethod](t) => { _tweedRepositoryMock.Verify(tr => tr.Create(It.IsAny<Tweed>())); },
            e => Assert.Fail(e.Message));
    }

    [Fact]
    public async Task CreateReplyTweed_SetsThreadId()
    {
        Tweed parentTweed = new(id: "parentTweedId", threadId: "threadId", authorId: "authorId",
            createdAt: FixedDateTime, text: string.Empty);
        _tweedRepositoryMock.Setup(t => t.GetById(parentTweed.Id!)).ReturnsAsync(parentTweed);

        var result = await _sut.CreateReplyTweed("authorId", "text", FixedDateTime, "parentTweedId");

        result.Switch(
            [AssertionMethod](t) => { Assert.Equal(parentTweed.ThreadId, t.ThreadId); },
            e => Assert.Fail(e.Message));
    }
}
