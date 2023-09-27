using OneOf;
using Tailors.Thread.Domain.ThreadAggregate;

namespace Tailors.Thread.Domain.TweedAggregate;

public interface ICreateTweedUseCase
{
    Task<OneOf<Tweed>> CreateRootTweed(string authorId, string text, DateTime createdAt);

    Task<OneOf<Tweed, DomainError>> CreateReplyTweed(string authorId, string text,
        DateTime createdAt, string parentTweedId);
}

public class CreateTweedUseCase : ICreateTweedUseCase
{
    private readonly ITweedRepository _tweedRepository;
    private readonly ITweedThreadRepository _tweedThreadRepository;

    public CreateTweedUseCase(ITweedRepository tweedRepository,
        ITweedThreadRepository tweedThreadRepository)
    {
        _tweedRepository = tweedRepository;
        _tweedThreadRepository = tweedThreadRepository;
    }

    public async Task<OneOf<Tweed>> CreateRootTweed(string authorId, string text,
        DateTime createdAt)
    {
        Tweed tweed = new(authorId: authorId, text: text, createdAt: createdAt);
        await _tweedRepository.Create(tweed);

        var thread = await CreateThread(tweed);
        tweed.ThreadId = thread.Id;
        return tweed;
    }

    public async Task<OneOf<Tweed, DomainError>> CreateReplyTweed(string authorId,
        string text,
        DateTime createdAt, string parentTweedId)
    {
        var parentTweed = await _tweedRepository.GetById(parentTweedId);
        if (parentTweed is null)
            return new DomainError($"Parent Tweed {parentTweedId} not found");
        var threadId = parentTweed.ThreadId;
        Tweed tweed = new(authorId: authorId, text: text, createdAt: createdAt, parentTweedId: parentTweedId, threadId: threadId);
        await _tweedRepository.Create(tweed);
        return tweed;
    }

    private async Task<TweedThread> CreateThread(Tweed tweed)
    {
        TweedThread thread = new();
        thread.AddTweed(tweed);
        await _tweedThreadRepository.Create(thread);
        return thread;
    }
}
