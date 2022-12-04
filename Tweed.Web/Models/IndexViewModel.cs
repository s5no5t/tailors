using Tweed.Web.Pages.Shared;

namespace Tweed.Web.Models;

public class IndexViewModel
{
    public List<TweedViewModel> Tweeds { get; init; } = new();
}
