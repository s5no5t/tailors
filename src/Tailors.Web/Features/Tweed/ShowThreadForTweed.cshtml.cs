using Tailors.Web.Features.Shared;

namespace Tailors.Web.Features.Tweed;

public class ShowThreadForTweedViewModel
{
    public List<TweedViewModel> LeadingTweeds { get; set; } = new();
    public CreateTweedViewModel CreateReplyTweed { get; set; } = new();
    public List<TweedViewModel> ReplyTweeds { get; set; } = new();
}
