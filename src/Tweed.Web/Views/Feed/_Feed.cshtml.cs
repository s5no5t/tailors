using Tweed.Web.Views.Shared;

namespace Tweed.Web.Views.Feed;

public class FeedViewModel
{
    public int Page { get; set; }
    public List<TweedViewModel> Tweeds { get; init; } = new();
    public bool NextPageExists { get; set; } = true;
}
