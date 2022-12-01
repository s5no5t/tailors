using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Tweed.Web.Pages;

[Authorize]
public class ProfilePageModel : PageModel
{
    public void OnGet()
    {
    }
}
