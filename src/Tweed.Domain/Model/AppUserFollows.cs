using NodaTime;

namespace Tweed.Domain.Model;

public class AppUserFollows
{
    public string? AppUserId { get; set; }
    public List<LeaderReference> Follows { get; set; } = new();

    public static string BuildId(string appUserId)
    {
        ArgumentNullException.ThrowIfNull(appUserId);
        return $"{appUserId}/Follows";
    }

    public class LeaderReference
    {
        public string? LeaderId { get; set; }
        public ZonedDateTime CreatedAt { get; set; }
    }
}