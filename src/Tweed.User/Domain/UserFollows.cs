namespace Tweed.User.Domain;

public class UserFollows
{
    public string? UserId { get; set; }
    public List<LeaderReference> Follows { get; set; } = new();

    public static string BuildId(string userId)
    {
        ArgumentNullException.ThrowIfNull(userId);
        return $"{userId}/Follows";
    }

    public class LeaderReference
    {
        public string? LeaderId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
