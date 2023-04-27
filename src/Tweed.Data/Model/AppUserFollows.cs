using NodaTime;

namespace Tweed.Data.Model;

public class AppUserFollows
{
    public string? AppUserId { get; set; }
    public List<Follows> Follows { get; set; } = new();

    public static string BuildId(string appUserId)
    {
        ArgumentNullException.ThrowIfNull(appUserId);
        return $"{appUserId}/Follows";
    }
}

public class Follows
{
    public string? LeaderId { get; set; }
    public ZonedDateTime CreatedAt { get; set; }
}
