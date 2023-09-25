namespace Tailors.Thread.Domain.TweedAggregate;

public class Tweed
{
    public Tweed(string? id = null, string? parentTweedId = null, string? threadId = null, string? text = null, string? authorId = null, DateTime? createdAt = null)
    {
        ParentTweedId = parentTweedId;
        ThreadId = threadId;
        Id = id;
        Text = text;
        AuthorId = authorId;
        CreatedAt = createdAt;
    }

    public string? Id { get; }
    public string? ParentTweedId { get; }
    public string? Text { get; }
    public DateTime? CreatedAt { get; init; }
    public string? AuthorId { get; }
    public string? ThreadId { get; set; }
}
