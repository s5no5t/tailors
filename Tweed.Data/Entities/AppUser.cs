using NodaTime;
using Raven.Identity;

namespace Tweed.Data.Entities;

public class AppUser : IdentityUser
{
    public List<Follows> Follows { get; set; }
}

public class Follows
{
    public string LeaderId { get; set; }
    public ZonedDateTime CreatedAt { get; set; }
}
