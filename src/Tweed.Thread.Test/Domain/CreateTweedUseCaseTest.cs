using FluentResults.Extensions.FluentAssertions;
using Moq;
using NodaTime;
using Tweed.Thread.Domain;
using Xunit;

namespace Tweed.Thread.Test.Domain;

public class CreateTweedUseCaseTest
{
    private static readonly ZonedDateTime FixedZonedDateTime =
        new(new LocalDateTime(2022, 11, 18, 15, 20), DateTimeZone.Utc, new Offset());

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
        var result = await _sut.CreateRootTweed("authorId", "text", FixedZonedDateTime);

        result.Should().BeSuccess();
        _tweedRepositoryMock.Verify(s => s.Create(It.IsAny<Thread.Domain.Tweed>()));
    }

    [Fact]
    public async Task CreateRootTweed_CreatesThread()
    {
        var result = await _sut.CreateRootTweed("authorId", "text", FixedZonedDateTime);

        result.Should().BeSuccess();
        _tweedThreadRepositoryMock.Verify(s => s.Create(It.IsAny<TweedThread>()));
    }

    [Fact]
    public async Task CreateReplyTweed_SavesTweed()
    {
        Thread.Domain.Tweed parentTweed = new()
        {
            Id = "parentTweedId",
            ThreadId = "threadId"
        };
        _tweedRepositoryMock.Setup(t => t.GetById(parentTweed.Id)).ReturnsAsync(parentTweed);

        var result = await _sut.CreateReplyTweed("authorId", "text", FixedZonedDateTime, parentTweed.Id);

        result.Should().BeSuccess();
        _tweedRepositoryMock.Verify(t => t.Create(It.IsAny<Thread.Domain.Tweed>()));
    }

    [Fact]
    public async Task CreateReplyTweed_SetsThreadId()
    {
        Thread.Domain.Tweed parentTweed = new()
        {
            Id = "parentTweedId",
            ThreadId = "threadId"
        };
        _tweedRepositoryMock.Setup(t => t.GetById(parentTweed.Id)).ReturnsAsync(parentTweed);

        var result = await _sut.CreateReplyTweed("authorId", "text", FixedZonedDateTime, "parentTweedId");

        result.Should().BeSuccess();
        Assert.Equal(parentTweed.ThreadId, result.Value.ThreadId);
    }
}