using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tweed.Data;

namespace Tweed.Web.Pages;

public class CreateModel : PageModel
{
    private readonly ITweedQueries _tweedQueries;

    public CreateModel(ITweedQueries tweedQueries)
    {
        _tweedQueries = tweedQueries;
    }

    [BindProperty] public Data.Models.Tweed? Tweed { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid || Tweed is null) return Page();

        await _tweedQueries.CreateTweed(Tweed);

        return RedirectToPage("./index");
    }
}
