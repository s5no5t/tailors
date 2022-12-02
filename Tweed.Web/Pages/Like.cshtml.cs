using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NodaTime;
using Tweed.Data;
using Tweed.Data.Entities;

namespace Tweed.Web.Pages;

[Authorize]
public class LikePageModel : PageModel
{
    private readonly ITweedQueries _tweedQueries;
    private readonly UserManager<AppUser> _userManager;

    public LikePageModel(ITweedQueries tweedQueries, UserManager<AppUser> userManager)
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
        var now = SystemClock.Instance.GetCurrentInstant().InUtc();
        await _tweedQueries.AddLike(Id!, userId, now);

        var referer = Request.Headers.Referer.ToString();
        if (referer == string.Empty)
            return new NoContentResult();

        return Redirect(referer);
    }
}
