using Newtonsoft.Json;

namespace Tailors.Domain.UserLikesAggregate;

public class UserLikes
{
    private readonly List<TweedLike> _likes = new();

    public UserLikes(string userId)
    {
        UserId = userId;
    }

    [JsonConstructor]
    public UserLikes(string userId, List<TweedLike> likes)
    {
        UserId = userId;
        _likes = likes;
    }

    public string? Id { get; set; }

    public string UserId { get; }
    public IReadOnlyList<TweedLike> Likes => _likes;

    public bool AddLike(string tweedId, DateTime likedAt)
    {
        if (_likes.Any(l => l.TweedId == tweedId))
            return false;
        _likes.Add(new TweedLike(tweedId, likedAt));
        return true;
    }

    public bool RemoveLike(string tweedId)
    {
        var removedCount = _likes.RemoveAll(lb => lb.TweedId == tweedId);
        return removedCount > 0;
    }

    public static string BuildId(string userId)
    {
        ArgumentNullException.ThrowIfNull(userId);
        return $"{userId}/Likes";
    }

    public class TweedLike(string tweedId, DateTime createdAt)
    {
        public string TweedId { get; } = tweedId;
        public DateTime CreatedAt { get; } = createdAt;
    }
}
