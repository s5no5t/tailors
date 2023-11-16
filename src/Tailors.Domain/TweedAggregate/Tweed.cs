namespace Tailors.Domain.TweedAggregate;

public class Tweed(string authorId, string text, DateTime createdAt, string? id = null, string? parentTweedId = null,
    string? threadId = null)
{
    public string? Id { get; set; } = id;
    public string? ParentTweedId { get; } = parentTweedId;
    public string Text { get; } = text;
    public DateTime CreatedAt { get; } = createdAt;
    public string AuthorId { get; } = authorId;
    public string? ThreadId { get; set; } = threadId;
}
