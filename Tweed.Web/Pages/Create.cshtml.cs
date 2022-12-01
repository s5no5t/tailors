using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NodaTime;
using Tweed.Data;
using Tweed.Data.Entities;
using Tweed.Web.Helper;

namespace Tweed.Web.Pages;

[Authorize]
public class CreateModel : PageModel
{
    private readonly INotificationManager _notificationManager;
    private readonly ITweedQueries _tweedQueries;
    private readonly UserManager<AppUser> _userManager;

    public CreateModel(ITweedQueries tweedQueries, UserManager<AppUser> userManager,
        INotificationManager notificationManager)
    {
        _tweedQueries = tweedQueries;
        _userManager = userManager;
        _notificationManager = notificationManager;
    }

    [BindProperty]
    [Required]
    [StringLength(280)]
    public string? Text { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var userId = _userManager.GetUserId(User);
        var now = SystemClock.Instance.GetCurrentInstant().InUtc();
        await _tweedQueries.StoreTweed(Text!, userId, now);

        _notificationManager.AppendSuccess("Tweed Posted");

        return RedirectToPage("./index");
    }
}
