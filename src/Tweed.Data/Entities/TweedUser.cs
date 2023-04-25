namespace Tweed.Data.Entities;

public class TweedUser
{
    public string? Id { get; set; }
    public string? IdentityUserId { get; set; }
    public List<Follows> Follows { get; set; } = new();
    public List<TweedLike> Likes { get; set; } = new();
}
