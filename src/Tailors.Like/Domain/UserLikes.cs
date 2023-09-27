using Newtonsoft.Json;

namespace Tailors.Like.Domain;

public class UserLikes
{
    private readonly List<TweedLike> _likes = new ();

    public UserLikes(string? userId)
    {
        UserId = userId;
    }

    [JsonConstructor]
    public UserLikes(string? userId, List<TweedLike> likes)
    {
        UserId = userId;
        _likes = likes;
    }

    public string? UserId { get; }

    public IReadOnlyList<TweedLike> Likes  => _likes;
    
    public bool AddLike(string tweedId, DateTime likedAt)
    {
        if (_likes.Any(l => l.TweedId == tweedId))
            return false;
        _likes.Add(new TweedLike
        {
            TweedId = tweedId,
            CreatedAt = likedAt
        });
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

    public class TweedLike
    {
        public string? TweedId { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
