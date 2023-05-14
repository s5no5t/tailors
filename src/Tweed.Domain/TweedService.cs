using NodaTime;
using Tweed.Domain.Model;

namespace Tweed.Domain;

public interface ITweedService
{
    Task<Model.Tweed> CreateRootTweed(string authorId, string text, ZonedDateTime createdAt);

    Task<Model.Tweed> CreateReplyTweed(string authorId, string text, ZonedDateTime createdAt,
        string parentTweedId);
}

public class TweedService : ITweedService
{
    private readonly ITweedRepository _tweedRepository;
    private readonly ITweedThreadRepository _tweedThreadRepository;

    public TweedService(ITweedRepository tweedRepository, ITweedThreadRepository tweedThreadRepository)
    {
        _tweedRepository = tweedRepository;
        _tweedThreadRepository = tweedThreadRepository;
    }

    public async Task<Model.Tweed> CreateRootTweed(string authorId, string text,
        ZonedDateTime createdAt)
    {
        Model.Tweed tweed = new()
        {
            AuthorId = authorId,
            Text = text,
            CreatedAt = createdAt
        };
        await _tweedRepository.Create(tweed);

        var thread = await CreateThread(tweed.Id!);
        tweed.ThreadId = thread.Id;
        return tweed;
    }

    public async Task<Model.Tweed> CreateReplyTweed(string authorId, string text,
        ZonedDateTime createdAt, string parentTweedId)
    {
        var parentTweed = await _tweedRepository.GetTweedById(parentTweedId);
        if (parentTweed is null)
            throw new Exception($"Parent Tweed {parentTweedId} not found");
        var threadId = parentTweed.ThreadId;
        Model.Tweed tweed = new()
        {
            AuthorId = authorId,
            Text = text,
            ParentTweedId = parentTweedId,
            ThreadId = threadId,
            CreatedAt = createdAt
        };
        await _tweedRepository.Create(tweed);
        return tweed;
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