namespace Tweed.Web.ViewModels;

public class ProfileViewModel
{
    public string? UserName { get; set; }

    public List<TweedViewModel> Tweeds { get; init; } = new();
}