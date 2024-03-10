namespace Tailors.Web.Features.Tweed;

public class TweedViewModel
{
    public string? AuthorId { get; init; }
    public string? Author { get; init; }
    public string? Text { get; init; }
    public string? CreatedAt { get; init; }
    public string? Id { get; init; }
    public long? LikesCount { get; init; }
    public bool LikedByCurrentUser { get; init; }
    public bool IsCurrentTweed { get; init; }
}
