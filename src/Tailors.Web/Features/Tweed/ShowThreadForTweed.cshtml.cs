using Tailors.Web.Features.Shared;

namespace Tailors.Web.Features.Tweed;

public class ShowThreadForTweedViewModel
{
    public List<TweedViewModel> LeadingTweeds { get; set; } = new();
    public CreateReplyTweedViewModel CreateReplyTweed { get; set; } = new();
}
