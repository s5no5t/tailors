using Tailors.Web.Features.Shared;

namespace Tailors.Web.Features.Tweed;

public class ShowThreadForTweedViewModel
{
    public List<TweedViewModel> Tweeds { get; set; } = new();
    public CreateReplyTweedViewModel CreateReplyTweed { get; set; } = new();
}
