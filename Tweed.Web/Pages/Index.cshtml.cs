using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Tweed.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public List<Data.Models.Tweed> Tweeds { get; } = new();

    public void OnGet()
    {
        Tweeds.Add(new Data.Models.Tweed { Content = "test" });
        Tweeds.Add(new Data.Models.Tweed { Content = "test2" });
    }
}
