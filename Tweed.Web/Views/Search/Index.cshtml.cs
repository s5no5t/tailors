namespace Tweed.Web.Views.Search;

public class IndexViewModel
{
    public List<UserViewModel> FoundUsers { get; set; } = new();
    public string? Term { get; set; }
}

public class UserViewModel
{
    public string UserId { get; set; }
    public string UserName { get; set; }
}



