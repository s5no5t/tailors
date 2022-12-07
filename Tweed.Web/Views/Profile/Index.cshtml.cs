using Tweed.Web.Views.Shared;

namespace Tweed.Web.Views.Profile;

public class IndexViewModel
{
    public string? UserName { get; init; }
    public List<TweedViewModel> Tweeds { get; init; } = new();
    public bool CurrentUserFollows { get; init; }
}
