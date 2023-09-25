namespace Tailors.Thread.Domain.TweedAggregate;

public class Tweed
{
    public Tweed(string? id = null)
    {
        Id = id;
    }

    public Tweed(string? id, string? text, string? authorId, DateTime? createdAt)
    {
        Id = id;
        Text = text;
        AuthorId = authorId;
        CreatedAt = createdAt;
    }

    public Tweed(string? id, string? threadId) : this()
    {
        Id = id;
        ThreadId = threadId;
    }

    public Tweed(string? id, string? parentTweedId, string? threadId) : this()
    {
        Id = id;
        ParentTweedId = parentTweedId;
        ThreadId = threadId;
    }

    public Tweed(string? id, DateTime? createdAt) : this()
    {
        Id = id;
        CreatedAt = createdAt;
    }

    public string? Id { get; protected set; }
    public string? ParentTweedId { get; set; }
    public string? Text { get; init; }
    public DateTime? CreatedAt { get; init; }
    public string? AuthorId { get; set; }
    public string? ThreadId { get; set; }
}
