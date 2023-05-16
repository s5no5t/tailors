using Tweed.Web.Views.Shared;

namespace Tweed.Web.Views.Tweed;

public class ShowThreadForTweedViewModel
{
    public List<TweedViewModel> Tweeds { get; set; } = new();
    public CreateReplyTweedViewModel CreateReplyTweed { get; set; } = new();
}
