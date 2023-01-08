using System.Collections;
using NodaTime;
using Raven.Identity;

namespace Tweed.Data.Entities;

public class AppUser : IdentityUser
{
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
