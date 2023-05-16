using NodaTime;

namespace Tweed.Like.Domain;

public interface ILikeTweedUseCase
{
    Task AddLike(string tweedId, string userId, ZonedDateTime likedAt);
    Task RemoveLike(string tweedId, string userId);
    Task<bool> DoesUserLikeTweed(string tweedId, string userId);
}

public class LikeTweedUseCase : ILikeTweedUseCase
{
    private readonly ITweedLikesRepository _tweedLikesRepository;

    public LikeTweedUseCase(ITweedLikesRepository tweedLikesRepository)
    {
        _tweedLikesRepository = tweedLikesRepository;
    }

    public async Task AddLike(string tweedId, string userId, ZonedDateTime likedAt)
    {
        var userLikes = await GetOrCreateUserLikes(userId);

        if (userLikes.Likes.Any(l => l.TweedId == tweedId))
            return;
        userLikes.Likes.Add(new UserLikes.TweedLike
        {
            TweedId = tweedId,
            CreatedAt = likedAt
        });

        _tweedLikesRepository.IncreaseLikesCounter(tweedId);
    }

    public async Task RemoveLike(string tweedId, string userId)
    {
        var userLikes = await GetOrCreateUserLikes(userId);
        userLikes.Likes.RemoveAll(lb => lb.TweedId == tweedId);

        _tweedLikesRepository.DecreaseLikesCounter(tweedId);
    }

    public async Task<bool> DoesUserLikeTweed(string tweedId, string userId)
    {
        var userLikes = await GetOrCreateUserLikes(userId);
        return userLikes.Likes.Any(lb => lb.TweedId == tweedId);
    }

    private async Task<UserLikes> GetOrCreateUserLikes(string userId)
    {
        var userLikesId = UserLikes.BuildId(userId);
        var userLikes = await _tweedLikesRepository.GetById(userLikesId);
        if (userLikes is null)
        {
            userLikes = new UserLikes
            {
                UserId = userId
            };
            await _tweedLikesRepository.Create(userLikes);
        }

        return userLikes;
    }
}
