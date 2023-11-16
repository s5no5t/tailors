using OneOf;

namespace Tailors.Domain.TweedAggregate;

public class CreateTweedUseCase(ITweedRepository tweedRepository)
{
    public async Task<OneOf<Tweed>> CreateRootTweed(string authorId, string text,
        DateTime createdAt)
    {
        Tweed tweed = new(authorId, text, createdAt);
        await tweedRepository.Create(tweed);
        return tweed;
    }

    public async Task<OneOf<Tweed, ResourceNotFoundError>> CreateReplyTweed(string authorId,
        string text,
        DateTime createdAt, string parentTweedId)
    {
        var getParentTweedResult = await tweedRepository.GetById(parentTweedId);
        if (getParentTweedResult.TryPickT1(out _, out var parentTweed))
            return new ResourceNotFoundError($"Tweed {parentTweedId} not found");

        var threadId = parentTweed.ThreadId;
        Tweed tweed = new(authorId, text, createdAt, parentTweedId: parentTweedId,
            threadId: threadId);
        await tweedRepository.Create(tweed);
        return tweed;
    }
}
