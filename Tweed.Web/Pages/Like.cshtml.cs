using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tweed.Data;
using Tweed.Data.Entities;

namespace Tweed.Web.Pages;

[Authorize]
public class LikeModel : PageModel
{
    private readonly ITweedQueries _tweedQueries;
    private readonly UserManager<AppUser> _userManager;

    public LikeModel(ITweedQueries tweedQueries, UserManager<AppUser> userManager)
    {
        _tweedQueries = tweedQueries;
        _userManager = userManager;
    }

    [FromQuery] [Required] public string? Id { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var userId = _userManager.GetUserId(User);
        await _tweedQueries.AddLike(Id!, userId);

        return RedirectToPage("./index");
    }
}
