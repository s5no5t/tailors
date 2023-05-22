using Tweed.Web.Features.Shared;

namespace Tweed.Web.Features.Tweed;

public class ShowThreadForTweedViewModel
{
    public List<TweedViewModel> Tweeds { get; set; } = new();
    public CreateReplyTweedViewModel CreateReplyTweed { get; set; } = new();
}
