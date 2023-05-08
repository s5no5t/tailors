using Tweed.Web.Views.Shared;

namespace Tweed.Web.Views.Tweed;

public class GetByIdViewModel
{
    public List<TweedViewModel> LeadingTweeds { get; set; } = new();
    public TweedViewModel Tweed { get; set; } = new();
    public CreateReplyTweedViewModel CreateTweed { get; set; } = new();
}
