namespace Tailors.Web.Features.Search;

public class ResultsViewModel(
    string term,
    SearchKind kind,
    List<UserViewModel> foundUsers,
    List<TweedViewModel> foundTweeds)
{
    public string Term { get; } = term;

    public SearchKind Kind { get; } = kind;
    public List<UserViewModel> FoundUsers { get; init; } = foundUsers;
    public List<TweedViewModel> FoundTweeds { get; init; } = foundTweeds;
}

public class UserViewModel(string userId, string userName)
{
    public string UserId { get; } = userId;

    public string UserName { get; } = userName;
}

public class TweedViewModel(string tweedId, string text)
{
    public string TweedId { get; } = tweedId;

    public string Text { get; } = text;
}
