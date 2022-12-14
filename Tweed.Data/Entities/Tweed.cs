using NodaTime;

namespace Tweed.Data.Entities;

public class Tweed
{
    public static readonly string LikesCounterName = "Likes";

    public string Id { get; set; }
    public string? Text { get; init; }
    public ZonedDateTime? CreatedAt { get; init; }
    public string? AuthorId { get; set; }
}
