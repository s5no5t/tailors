namespace Tweed.Web.Views.Search;

public class IndexViewModel
{
    public List<UserViewModel> Users { get; set; } = new();
}

public class UserViewModel
{
    public string UserId { get; set; }
}



