using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Tweed.Web.Pages;

public class CreateModel : PageModel
{
    [BindProperty] public Models.Tweed? Tweed { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        return RedirectToPage("./index");
    }
}
