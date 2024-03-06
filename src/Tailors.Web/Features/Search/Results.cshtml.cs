namespace Tailors.Web.Features.Search;

public class ResultsViewModel(string? term, List<UserViewModel> foundUsers, List<TweedViewModel> foundTweeds)
{
    public string? Term { get; set; } = term;
    public List<UserViewModel> FoundUsers { get; init; } = foundUsers;
    public List<TweedViewModel> FoundTweeds { get; init; } = foundTweeds;
}

public class UserViewModel(string userId, string userName)
{
    public string UserId { get; set; } = userId;

    public string UserName { get; set; } = userName;
}

public class TweedViewModel(string tweedId, string text)
{
    public string TweedId { get; set; } = tweedId;

    public string Text { get; set; } = text;
}
