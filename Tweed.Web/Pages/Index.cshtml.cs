using Microsoft.AspNetCore.Mvc.RazorPages;
using Tweed.Data;

namespace Tweed.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ITweedQueries _tweedQueries;

    public IndexModel(ITweedQueries tweedQueries)
    {
        _tweedQueries = tweedQueries;
    }

    public List<Data.Models.Tweed> Tweeds { get; } = new();

    public async Task OnGetAsync()
    {
        var latestTweeds = await _tweedQueries.GetLatestTweeds();
        Tweeds.AddRange(latestTweeds);
    }
}
