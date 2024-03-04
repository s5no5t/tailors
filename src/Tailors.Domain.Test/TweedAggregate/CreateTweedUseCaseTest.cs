using JetBrains.Annotations;
using Tailors.Domain.TweedAggregate;

namespace Tailors.Domain.Test.TweedAggregate;

public class CreateTweedUseCaseTest
{
    private readonly CreateTweedUseCase _sut;
    private readonly TweedRepositoryMock _tweedRepositoryMock = new();

    public CreateTweedUseCaseTest()
    {
        _sut = new CreateTweedUseCase(_tweedRepositoryMock);
    }

    [Fact]
    public async Task CreateRootTweed_SavesTweed()
    {
        var result = await _sut.CreateRootTweed("authorId", "text", TestData.FixedDateTime);

        var tweed = await _tweedRepositoryMock.GetById(result.AsT0.Id!);
        Assert.NotNull(tweed.AsT0);
    }

    [Fact]
    public async Task CreateReplyTweed_SavesTweed()
    {
        Tweed parentTweed = new(id: "parentTweedId", authorId: "authorId", createdAt: TestData.FixedDateTime,
            text: string.Empty);
        await _tweedRepositoryMock.Create(parentTweed);

        var result = await _sut.CreateReplyTweed("authorId", "text", TestData.FixedDateTime, parentTweed.Id!);

        var tweed = await _tweedRepositoryMock.GetById(result.AsT0.Id!);
        Assert.NotNull(tweed.AsT0);
    }

    [Fact]
    public async Task CreateReplyTweed_SetsLeadingThreadIds()
    {
        Tweed parentTweed = new(id: "parentTweedId", authorId: "authorId", createdAt: TestData.FixedDateTime,
            text: string.Empty);
        await _tweedRepositoryMock.Create(parentTweed);

        var result = await _sut.CreateReplyTweed("authorId", "text", TestData.FixedDateTime, "parentTweedId");

        result.Switch(
            [AssertionMethod] (t) => { Assert.Equal(parentTweed.Id, t.LeadingTweedIds[0]); },
            e => Assert.Fail(e.Message));
    }
}
