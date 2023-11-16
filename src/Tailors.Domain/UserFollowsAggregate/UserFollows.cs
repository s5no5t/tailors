using Newtonsoft.Json;

namespace Tailors.Domain.UserFollowsAggregate;

public class UserFollows
{
    private readonly List<LeaderReference> _follows = new();

    public UserFollows(string userId)
    {
        UserId = userId;
    }

    [JsonConstructor]
    public UserFollows(string userId, List<LeaderReference> follows)
    {
        UserId = userId;
        _follows = follows;
    }

    public string? Id { get; set; }
    public string UserId { get; }
    public IReadOnlyList<LeaderReference> Follows => _follows;

    public bool AddFollows(string leaderId, DateTime createdAt)
    {
        if (_follows.Any(f => f.LeaderId == leaderId))
            return false;

        _follows.Add(new LeaderReference(leaderId, createdAt));
        return true;
    }

    public void RemoveFollows(string leaderId)
    {
        _follows.RemoveAll(f => f.LeaderId == leaderId);
    }

    public static string BuildId(string userId)
    {
        ArgumentNullException.ThrowIfNull(userId);
        return $"{userId}/Follows";
    }

    public class LeaderReference(string leaderId, DateTime createdAt)
    {
        public string LeaderId { get; } = leaderId;
        public DateTime CreatedAt { get; } = createdAt;
    }
}
