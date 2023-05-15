using System.Threading.Tasks;
using Moq;
using NodaTime;
using Tweed.Domain.Model;
using Xunit;

namespace Tweed.Domain.Test;

public class TweedServiceTest
{
    private static readonly ZonedDateTime FixedZonedDateTime =
        new(new LocalDateTime(2022, 11, 18, 15, 20), DateTimeZone.Utc, new Offset());

    private readonly TweedService _sut;
    private readonly Mock<ITweedRepository> _tweedRepositoryMock = new();
    private readonly Mock<ITweedThreadRepository> _tweedThreadRepositoryMock = new();

    public TweedServiceTest()
    {
        _sut = new TweedService(_tweedRepositoryMock.Object, _tweedThreadRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateRootTweed_SavesTweed()
    {
        await _sut.CreateRootTweed("authorId", "text", FixedZonedDateTime);

        _tweedRepositoryMock.Verify(s => s.Create(It.IsAny<Domain.Model.Tweed>()));
    }

    [Fact]
    public async Task CreateRootTweed_CreatesThread()
    {
        await _sut.CreateRootTweed("authorId", "text", FixedZonedDateTime);

        _tweedThreadRepositoryMock.Verify(s => s.Create(It.IsAny<TweedThread>()));
    }

    [Fact]
    public async Task CreateReplyTweed_SavesTweed()
    {
        Domain.Model.Tweed parentTweed = new()
        {
            Id = "parentTweedId",
            ThreadId = "threadId"
        };
        _tweedRepositoryMock.Setup(t => t.GetById(parentTweed.Id)).ReturnsAsync(parentTweed);

        var tweed = await _sut.CreateReplyTweed("authorId", "text", FixedZonedDateTime, parentTweed.Id);

        _tweedRepositoryMock.Verify(t => t.Create(It.IsAny<Domain.Model.Tweed>()));
    }

    [Fact]
    public async Task CreateReplyTweed_SetsThreadId()
    {
        Domain.Model.Tweed parentTweed = new()
        {
            Id = "parentTweedId",
            ThreadId = "threadId"
        };
        _tweedRepositoryMock.Setup(t => t.GetById(parentTweed.Id)).ReturnsAsync(parentTweed);

        var tweed = await _sut.CreateReplyTweed("authorId", "text", FixedZonedDateTime, "parentTweedId");

        Assert.Equal(parentTweed.ThreadId, tweed.ThreadId);
    }
}