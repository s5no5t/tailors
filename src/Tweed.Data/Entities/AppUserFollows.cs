using NodaTime;

namespace Tweed.Data.Entities;

public class AppUserFollows
{
    public string AppUserId { get; set; }
    public string Id { get; set; }
    public List<Follows> Follows { get; set; } = new();

    public static string BuildId(string appUserId) => $"AppUsers/{appUserId}/Follows";
}

public class Follows
{
    public string? LeaderId { get; set; }
    public ZonedDateTime CreatedAt { get; set; }
}