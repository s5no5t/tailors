using NodaTime;

namespace Tweed.Domain.Model;

public class UserLikes
{
    public string? UserId { get; set; }

    public List<TweedLike> Likes { get; set; } = new();

    public static string BuildId(string userId)
    {
        ArgumentNullException.ThrowIfNull(userId);
        return $"{userId}/Likes";
    }

    public class TweedLike
    {
        public string? TweedId { get; set; }
        public ZonedDateTime? CreatedAt { get; set; }
    }
}
