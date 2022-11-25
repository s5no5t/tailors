using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tweed.Data;

namespace Tweed.Web.Pages;

public class CreateModel : PageModel
{
    private readonly ITweedQueries _tweedQueries;
    private readonly UserManager<AppUser> _userManager;

    public CreateModel(ITweedQueries tweedQueries, UserManager<AppUser> userManager)
    {
        _tweedQueries = tweedQueries;
        _userManager = userManager;
    }

    [BindProperty] public Data.Models.Tweed? Tweed { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid || Tweed is null) return Page();

        var user = _userManager.GetUserId(User);
        await _tweedQueries.CreateTweed(Tweed, user);

        return RedirectToPage("./index");
    }
}
