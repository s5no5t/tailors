using Moq;
using Tailors.Thread.Domain;
using Xunit;

namespace Tailors.Thread.Test.Domain;

public class CreateTweedUseCaseTest
{
    private static readonly DateTime FixedDateTime = new DateTime(2022, 11, 18, 15, 20, 0);

    private readonly CreateTweedUseCase _sut;
    private readonly Mock<ITweedRepository> _tweedRepositoryMock = new();
    private readonly Mock<ITweedThreadRepository> _tweedThreadRepositoryMock = new();

    public CreateTweedUseCaseTest()
    {
        _sut = new CreateTweedUseCase(_tweedRepositoryMock.Object, _tweedThreadRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateRootTweed_SavesTweed()
    {
        var result = await _sut.CreateRootTweed("authorId", "text", FixedDateTime);

        Assert.True(result.IsT0);
        _tweedRepositoryMock.Verify(s => s.Create(It.IsAny<Tailors.Thread.Domain.Tweed>()));
    }

    [Fact]
    public async Task CreateRootTweed_CreatesThread()
    {
        var result = await _sut.CreateRootTweed("authorId", "text", FixedDateTime);

        Assert.True(result.IsT0);
        _tweedThreadRepositoryMock.Verify(s => s.Create(It.IsAny<TweedThread>()));
    }

    [Fact]
    public async Task CreateReplyTweed_SavesTweed()
    {
        Tailors.Thread.Domain.Tweed parentTweed = new()
        {
            Id = "parentTweedId",
            ThreadId = "threadId"
        };
        _tweedRepositoryMock.Setup(t => t.GetById(parentTweed.Id)).ReturnsAsync(parentTweed);

        var result = await _sut.CreateReplyTweed("authorId", "text", FixedDateTime, parentTweed.Id);

        Assert.True(result.IsT0);
        _tweedRepositoryMock.Verify(t => t.Create(It.IsAny<Tailors.Thread.Domain.Tweed>()));
    }

    [Fact]
    public async Task CreateReplyTweed_SetsThreadId()
    {
        Tailors.Thread.Domain.Tweed parentTweed = new()
        {
            Id = "parentTweedId",
            ThreadId = "threadId"
        };
        _tweedRepositoryMock.Setup(t => t.GetById(parentTweed.Id)).ReturnsAsync(parentTweed);

        var result = await _sut.CreateReplyTweed("authorId", "text", FixedDateTime, "parentTweedId");

        Assert.True(result.IsT0);
        Assert.Equal(parentTweed.ThreadId, result.AsT0.ThreadId);
    }
}
