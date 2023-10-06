using Tailors.Web.Features.Shared;

namespace Tailors.Web.Features.Profile;

public class IndexViewModel
{
    public IndexViewModel(string userId, string userName, List<TweedViewModel> tweeds, bool currentUserFollows,
        int followersCount)
    {
        UserId = userId;
        UserName = userName;
        Tweeds = tweeds;
        CurrentUserFollows = currentUserFollows;
        FollowersCount = followersCount;
    }

    public string UserId { get; init; }
    public string UserName { get; init; }
    public List<TweedViewModel> Tweeds { get; init; } = new();
    public bool CurrentUserFollows { get; init; }
    public int FollowersCount { get; set; }
}
