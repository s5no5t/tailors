namespace Tailors.Thread.Domain.TweedAggregate;

public class Tweed
{
    public string? Id { get; set; }
    public string? ParentTweedId { get; set; }
    public string? Text { get; init; }
    public DateTime? CreatedAt { get; init; }
    public string? AuthorId { get; set; }
    public string? ThreadId { get; set; }
}
