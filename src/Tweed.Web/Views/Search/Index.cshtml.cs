namespace Tweed.Web.Views.Search;

public class IndexViewModel
{
    public IndexViewModel(string? term, List<UserViewModel> foundUsers, List<TweedViewModel> foundTweeds)
    {
        Term = term;
        FoundUsers = foundUsers;
        FoundTweeds = foundTweeds;
    }

    public string? Term { get; set; }
    public List<UserViewModel> FoundUsers { get; set; } = new();
    public List<TweedViewModel> FoundTweeds { get; set; } = new();
}

public class UserViewModel
{
    public UserViewModel(string userId, string userName)
    {
        UserId = userId;
        UserName = userName;
    }
    public string UserId { get; set; }

    public string UserName { get; set; }
}

public class TweedViewModel
{
    public TweedViewModel(string tweedId, string text)
    {
        TweedId = tweedId;
        Text = text;
    }

    public string TweedId { get; set; }

    public string Text { get; set; }
}

