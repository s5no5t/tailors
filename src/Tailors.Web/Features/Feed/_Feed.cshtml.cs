using Tailors.Web.Features.Shared;

namespace Tailors.Web.Features.Feed;

public class FeedViewModel
{
    public CreateTweedViewModel CreateTweed { get; set; } = new();
    public int Page { get; set; }
    public List<TweedViewModel> Tweeds { get; init; } = new();
    public bool NextPageExists { get; set; } = true;
}
