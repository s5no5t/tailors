using NodaTime;

namespace Tweed.Data.Models;

public class Tweed
{
    public string? Content { get; set; }
    public ZonedDateTime? CreatedAt { get; set; }
    public string? AuthorId { get; set; }
}
