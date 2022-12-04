using NodaTime;

namespace Tweed.Web.ViewModels.Shared;

public class TweedViewModel
{
    public string? AuthorId { get; set; }
    public string? Author { get; set; }
    public string? Text { get; set; }
    public ZonedDateTime? CreatedAt { get; set; }
    public string? Id { get; set; }
    public int? Likes { get; set; }
    public bool LikedByCurrentUser { get; set; }
}
