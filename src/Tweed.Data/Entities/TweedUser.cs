using NodaTime;

namespace Tweed.Data.Entities;

public class TweedUser
{
    public string? Id { get; set; }
    public string? IdentityUserId { get; set; }
    public List<Follows> Follows { get; set; } = new();
    public List<TweedLike> Likes { get; set; } = new();
}

public class Follows
{
    public string? LeaderId { get; set; }
    public ZonedDateTime CreatedAt { get; set; }
}

public class TweedLike
{
    public string? TweedId { get; set; }
    public ZonedDateTime? CreatedAt { get; set; }
}
