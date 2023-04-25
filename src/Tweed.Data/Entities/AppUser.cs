using System.Collections;
using NodaTime;
using Raven.Identity;

namespace Tweed.Data.Entities;

public class AppUser : IdentityUser
{
    public List<TweedLike> Likes { get; set; } = new();
}

public class TweedLike
{
    public string? TweedId { get; set; }
    public ZonedDateTime? CreatedAt { get; set; }
}
