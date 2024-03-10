using Tailors.Web.Features.Tweed;

namespace Tailors.Web.Features.Profile;

public class IndexViewModel(
    string userId,
    string userName,
    List<TweedViewModel> tweeds,
    bool currentUserFollows,
    int followersCount)
{
    public string UserId { get; init; } = userId;
    public string UserName { get; init; } = userName;
    public List<TweedViewModel> Tweeds { get; init; } = tweeds;
    public bool CurrentUserFollows { get; init; } = currentUserFollows;
    public int FollowersCount { get; init; } = followersCount;
}
