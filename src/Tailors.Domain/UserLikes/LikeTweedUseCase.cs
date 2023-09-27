namespace Tailors.Domain.UserLikes;

public interface ILikeTweedUseCase
{
    Task AddLike(string tweedId, string userId, DateTime likedAt);
    Task RemoveLike(string tweedId, string userId);
    Task<bool> DoesUserLikeTweed(string tweedId, string userId);
}

public class LikeTweedUseCase : ILikeTweedUseCase
{
    private readonly IUserLikesRepository _userLikesRepository;

    public LikeTweedUseCase(IUserLikesRepository userLikesRepository)
    {
        _userLikesRepository = userLikesRepository;
    }

    public async Task AddLike(string tweedId, string userId, DateTime likedAt)
    {
        var userLikes = await GetOrCreateUserLikes(userId);
        
        var added = userLikes.AddLike(tweedId, likedAt);

        if (added)
            _userLikesRepository.IncreaseLikesCounter(tweedId);
    }

    public async Task RemoveLike(string tweedId, string userId)
    {
        var userLikes = await GetOrCreateUserLikes(userId);
        var removed = userLikes.RemoveLike(tweedId);

        if (removed)
            _userLikesRepository.DecreaseLikesCounter(tweedId);
    }

    public async Task<bool> DoesUserLikeTweed(string tweedId, string userId)
    {
        var userLikes = await GetOrCreateUserLikes(userId);
        return userLikes.Likes.Any(lb => lb.TweedId == tweedId);
    }

    private async Task<UserLikes> GetOrCreateUserLikes(string userId)
    {
        var userLikesId = UserLikes.BuildId(userId);
        var userLikes = await _userLikesRepository.GetById(userLikesId);
        if (userLikes is null)
        {
            userLikes = new UserLikes(userId: userId);
            await _userLikesRepository.Create(userLikes);
        }

        return userLikes;
    }
}
