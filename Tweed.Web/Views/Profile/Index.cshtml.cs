using Tweed.Web.Views.Shared;

namespace Tweed.Web.Views.Profile;

public class IndexViewModel
{
    public string? UserName { get; set; }

    public List<TweedViewModel> Tweeds { get; init; } = new();
}
