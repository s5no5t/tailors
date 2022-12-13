namespace Tweed.Web.Views.Search;

public class IndexViewModel
{
    public string? Term { get; set; }
    public List<UserViewModel> FoundUsers { get; set; } = new();
    public List<TweedViewModel> FoundTweeds { get; set; } = new();
}

public class UserViewModel
{
    public string UserId { get; set; }
    public string UserName { get; set; }
}

public class TweedViewModel
{
    public string TweedId { get; set; }
    public string Text { get; set; }
}

