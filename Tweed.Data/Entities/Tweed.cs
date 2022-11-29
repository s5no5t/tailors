using NodaTime;

namespace Tweed.Data.Entities;

public class Tweed
{
    public string? Text { get; init; }
    public ZonedDateTime? CreatedAt { get; init; }
    public string? AuthorId { get; set; }
}
