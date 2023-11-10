using OneOf;

namespace Tailors.Domain.TweedAggregate;

public class CreateTweedUseCase
{
    private readonly ITweedRepository _tweedRepository;

    public CreateTweedUseCase(ITweedRepository tweedRepository)
    {
        _tweedRepository = tweedRepository;
    }

    public async Task<OneOf<Tweed>> CreateRootTweed(string authorId, string text,
        DateTime createdAt)
    {
        Tweed tweed = new(authorId, text, createdAt);
        await _tweedRepository.Create(tweed);
        return tweed;
    }

    public async Task<OneOf<Tweed, ResourceNotFoundError>> CreateReplyTweed(string authorId,
        string text,
        DateTime createdAt, string parentTweedId)
    {
        var getParentTweedResult = await _tweedRepository.GetById(parentTweedId);
        if (getParentTweedResult.TryPickT1(out _, out var parentTweed))
            return new ResourceNotFoundError($"Tweed {parentTweedId} not found");

        var threadId = parentTweed.ThreadId;
        Tweed tweed = new(authorId, text, createdAt, parentTweedId: parentTweedId,
            threadId: threadId);
        await _tweedRepository.Create(tweed);
        return tweed;
    }
}
