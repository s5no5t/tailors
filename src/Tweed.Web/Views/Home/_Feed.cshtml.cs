using Tweed.Web.Views.Shared;

namespace Tweed.Web.Views.Home;

public class FeedViewModel
{
    public List<TweedViewModel> Tweeds { get; init; } = new();
    public int Page { get; set; }
}
