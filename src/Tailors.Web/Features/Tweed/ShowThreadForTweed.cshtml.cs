using Tailors.Web.Features.Shared;

namespace Tailors.Web.Features.Tweed;

public class ShowThreadForTweedViewModel
{
    public List<TweedViewModel> LeadingTweeds { get; init; } = [];
    public CreateTweedViewModel CreateReplyTweed { get; init; } = new();
    public List<TweedViewModel> ReplyTweeds { get; init; } = [];
}
