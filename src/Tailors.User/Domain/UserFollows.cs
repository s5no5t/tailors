using Newtonsoft.Json;

namespace Tailors.User.Domain;

public class UserFollows
{
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

    public string UserId { get; }
    
    private readonly List<LeaderReference> _follows = new();
    public IReadOnlyList<LeaderReference> Follows  => _follows;
    
    public bool AddFollows(string leaderId, DateTime createdAt)
    {
        if (_follows.Any(f => f.LeaderId == leaderId))
            return false;

        _follows.Add(new LeaderReference(leaderId: leaderId, createdAt: createdAt));
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

    public class LeaderReference
    {
        public LeaderReference(string leaderId, DateTime createdAt)
        {
            LeaderId = leaderId;
            CreatedAt = createdAt;
        }

        public string LeaderId { get; }
        public DateTime CreatedAt { get; }
    }
}
