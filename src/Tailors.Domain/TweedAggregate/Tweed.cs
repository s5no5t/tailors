using Newtonsoft.Json;

namespace Tailors.Domain.TweedAggregate;

public class Tweed(string authorId, string text, DateTime createdAt, string? id = null)
{
    [JsonConstructor]
    public Tweed(string authorId, string text, DateTime createdAt, List<string> leadingTweedIds, string? id = null)
        : this(authorId, text, createdAt, id)
    {
        _leadingTweedIds = leadingTweedIds;
    }

    public string? Id { get; set; } = id;
    public string Text { get; } = text;
    public DateTime CreatedAt { get; } = createdAt;
    public string AuthorId { get; } = authorId;

    private readonly List<string> _leadingTweedIds = [];
    public IReadOnlyList<string> LeadingTweedIds => _leadingTweedIds;

    public void AddLeadingTweedId(string leadingTweedId)
    {
        _leadingTweedIds.Add(leadingTweedId);
    }

    public void AddLeadingTweedIds(IReadOnlyList<string> leadingTweedIds)
    {
        _leadingTweedIds.AddRange(leadingTweedIds);
    }
}
