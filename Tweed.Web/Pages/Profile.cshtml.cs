using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tweed.Data;
using Tweed.Data.Entities;

namespace Tweed.Web.Pages;

[Authorize]
public class ProfilePageModel : PageModel
{
    private readonly ITweedQueries _tweedQueries;
    private readonly UserManager<AppUser> _userManager;

    public ProfilePageModel(ITweedQueries tweedQueries, UserManager<AppUser> userManager)
    {
        _tweedQueries = tweedQueries;
        _userManager = userManager;
    }

    public async Task OnGetAsync()
    {
        var tweeds = _tweedQueries.GetTweedsForUser("user1");
    }
}
