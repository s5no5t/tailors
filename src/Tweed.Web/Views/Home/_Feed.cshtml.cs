using Tweed.Web.Views.Shared;

namespace Tweed.Web.Views.Home;

public class FeedViewModel
{
    public int Page { get; set; }
    public List<TweedViewModel> Tweeds { get; init; } = new();
    public bool NextPageExists { get; set; } = true;
}
