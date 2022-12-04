using Tweed.Web.ViewModels.Shared;

namespace Tweed.Web.ViewModels;

public class HomeIndexViewModel
{
    public List<TweedViewModel> Tweeds { get; init; } = new();
}
