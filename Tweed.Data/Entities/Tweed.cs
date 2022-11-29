using NodaTime;

namespace Tweed.Data.Entities;

public class Tweed
{
    public string Id { get; set; }
    public string? Text { get; init; }
    public ZonedDateTime? CreatedAt { get; init; }
    public string? AuthorId { get; set; }
    public List<LikedBy> LikedBy { get; set; } = new();
}

public class LikedBy
{
    public string? UserId { get; set; }
    public ZonedDateTime? LikedAt { get; set; }
}
