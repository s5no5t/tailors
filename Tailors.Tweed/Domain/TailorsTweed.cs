namespace Tailors.Tweed.Domain;

public class TailorsTweed
{
    public TailorsTweed(string authorId, string text, DateTime createdAt, string? id = null, string? parentTweedId = null, string? threadId = null)
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
    public DateTime? CreatedAt { get; }
    public string? AuthorId { get; }
    public string? ThreadId { get; set; }
}