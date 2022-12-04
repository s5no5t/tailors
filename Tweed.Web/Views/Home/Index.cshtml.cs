using Tweed.Web.Pages.Shared;

namespace Tweed.Web.Views.Home;

public class IndexViewModel
{
    public List<TweedViewModel> Tweeds { get; init; } = new();
}
