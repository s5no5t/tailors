using Tweed.Web.ViewModels.Shared;

namespace Tweed.Web.ViewModels;

public class ProfileIndexViewModel
{
    public string? UserName { get; set; }

    public List<TweedViewModel> Tweeds { get; init; } = new();
}
