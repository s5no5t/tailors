using Tailors.Web.Features.Tweed;

namespace Tailors.Web.Features.Feed;

public class FeedViewModel
{
    public CreateTweedViewModel CreateTweed { get; init; } = new();
    public int Page { get; init; }
    public List<TweedViewModel> Tweeds { get; init; } = [];
    public bool NextPageExists { get; init; } = true;
}
