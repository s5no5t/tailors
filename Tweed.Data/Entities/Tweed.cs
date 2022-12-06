using NodaTime;

namespace Tweed.Data.Entities;

public class Tweed
{
    public string Id { get; set; }
    public string? Text { get; init; }
    public ZonedDateTime? CreatedAt { get; init; }
    public string? AuthorId { get; set; }
    public List<Likes> Likes { get; set; } = new();
}

public class Likes
{
    public string? UserId { get; set; }
    public ZonedDateTime? CreatedAt { get; set; }
}
