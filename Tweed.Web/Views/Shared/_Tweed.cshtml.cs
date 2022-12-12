namespace Tweed.Web.Views.Shared;

public class TweedViewModel
{
    public string? AuthorId { get; set; }
    public string? Author { get; set; }
    public string? Text { get; set; }
    public string? CreatedAt { get; set; }
    public string? Id { get; set; }
    public long? LikesCount { get; set; }
    public bool LikedByCurrentUser { get; set; }
}
