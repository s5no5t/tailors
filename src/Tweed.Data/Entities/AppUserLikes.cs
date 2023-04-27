using NodaTime;

namespace Tweed.Data.Entities;

public class AppUserLikes
{
    public string? AppUserId { get; set; }

    public List<TweedLike> Likes { get; set; } = new();

    public static string BuildId(string appUserId)
    {
        ArgumentNullException.ThrowIfNull(appUserId);
        return $"{appUserId}/Likes";
    }
}

public class TweedLike
{
    public string? TweedId { get; set; }
    public ZonedDateTime? CreatedAt { get; set; }
}
