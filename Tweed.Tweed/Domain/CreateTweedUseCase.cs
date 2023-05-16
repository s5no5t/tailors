using NodaTime;
using Tweed.Domain.Model;

namespace Tweed.Domain;

public interface ICreateTweedUseCase
{
    Task<TheTweed> CreateRootTweed(string authorId, string text, ZonedDateTime createdAt);

    Task<TheTweed> CreateReplyTweed(string authorId, string text, ZonedDateTime createdAt,
        string parentTweedId);
}

public class CreateTweedUseCase : ICreateTweedUseCase
{
    private readonly ITweedRepository _tweedRepository;
    private readonly ITweedThreadRepository _tweedThreadRepository;

    public CreateTweedUseCase(ITweedRepository tweedRepository, ITweedThreadRepository tweedThreadRepository)
    {
        _tweedRepository = tweedRepository;
        _tweedThreadRepository = tweedThreadRepository;
    }

    public async Task<TheTweed> CreateRootTweed(string authorId, string text,
        ZonedDateTime createdAt)
    {
        TheTweed theTweed = new()
        {
            AuthorId = authorId,
            Text = text,
            CreatedAt = createdAt
        };
        await _tweedRepository.Create(theTweed);

        var thread = await CreateThread(theTweed.Id!);
        theTweed.ThreadId = thread.Id;
        return theTweed;
    }

    public async Task<TheTweed> CreateReplyTweed(string authorId, string text,
        ZonedDateTime createdAt, string parentTweedId)
    {
        var parentTweed = await _tweedRepository.GetById(parentTweedId);
        if (parentTweed is null)
            throw new Exception($"Parent Tweed {parentTweedId} not found");
        var threadId = parentTweed.ThreadId;
        TheTweed theTweed = new()
        {
            AuthorId = authorId,
            Text = text,
            ParentTweedId = parentTweedId,
            ThreadId = threadId,
            CreatedAt = createdAt
        };
        await _tweedRepository.Create(theTweed);
        return theTweed;
    }

    private async Task<TweedThread> CreateThread(string tweedId)
    {
        TweedThread thread = new()
        {
            Root =
            {
                TweedId = tweedId
            }
        };
        await _tweedThreadRepository.Create(thread);
        return thread;
    }
}