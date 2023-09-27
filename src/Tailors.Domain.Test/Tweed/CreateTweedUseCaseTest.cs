using Moq;
using Tailors.Domain.Tweed;

namespace Tailors.Domain.Test.Tweed;

public class CreateTweedUseCaseTest
{
    private static readonly DateTime FixedDateTime = new DateTime(2022, 11, 18, 15, 20, 0);

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

        Assert.True(result.IsT0);
        _tweedRepositoryMock.Verify(s => s.Create(It.IsAny<TailorsTweed>()));
    }

    [Fact]
    public async Task CreateReplyTweed_SavesTweed()
    {
        TailorsTweed parentTweed = new(id: "parentTweedId", threadId: "threadId", authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        _tweedRepositoryMock.Setup(t => t.GetById(parentTweed.Id!)).ReturnsAsync(parentTweed);

        var result = await _sut.CreateReplyTweed("authorId", "text", FixedDateTime, parentTweed.Id!);

        Assert.True(result.IsT0);
        _tweedRepositoryMock.Verify(t => t.Create(It.IsAny<TailorsTweed>()));
    }

    [Fact]
    public async Task CreateReplyTweed_SetsThreadId()
    {
        TailorsTweed parentTweed = new(id: "parentTweedId", threadId: "threadId", authorId: "authorId", createdAt: FixedDateTime, text: string.Empty);
        _tweedRepositoryMock.Setup(t => t.GetById(parentTweed.Id!)).ReturnsAsync(parentTweed);

        var result = await _sut.CreateReplyTweed("authorId", "text", FixedDateTime, "parentTweedId");

        Assert.True(result.IsT0);
        Assert.Equal(parentTweed.ThreadId, result.AsT0.ThreadId);
    }
}
