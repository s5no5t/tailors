using Newtonsoft.Json;

namespace Tailors.Domain.UserLikes;

public class UserLikes
{
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

    public string UserId { get; }

    private readonly List<TweedLike> _likes = new ();
    public IReadOnlyList<TweedLike> Likes  => _likes;
    
    public bool AddLike(string tweedId, DateTime likedAt)
    {
        if (_likes.Any(l => l.TweedId == tweedId))
            return false;
        _likes.Add(new TweedLike(tweedId: tweedId, createdAt: likedAt));
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
        public TweedLike(string tweedId, DateTime createdAt)
        {
            TweedId = tweedId;
            CreatedAt = createdAt;
        }

        public string TweedId { get;  }
        public DateTime CreatedAt { get; }
    }
}
