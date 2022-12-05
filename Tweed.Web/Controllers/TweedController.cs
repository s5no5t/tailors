using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Tweed.Data;
using Tweed.Data.Entities;
using Tweed.Web.Helper;
using Tweed.Web.Views.Tweed;

namespace Tweed.Web.Controllers;

[Authorize]
public class TweedController : Controller
{
    private readonly INotificationManager _notificationManager;
    private readonly ITweedQueries _tweedQueries;
    private readonly UserManager<AppUser> _userManager;

    public TweedController(ITweedQueries tweedQueries, UserManager<AppUser> userManager,
        INotificationManager notificationManager)
    {
        _tweedQueries = tweedQueries;
        _userManager = userManager;
        _notificationManager = notificationManager;
    }

    public IActionResult Create()
    {
        CreateViewModel viewModel = new();
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateViewModel viewModel)
    {
        if (!ModelState.IsValid) return View();

        var userId = _userManager.GetUserId(User);
        var now = SystemClock.Instance.GetCurrentInstant().InUtc();

        await _tweedQueries.StoreTweed(viewModel.Text, userId, now);

        _notificationManager.AppendSuccess("Tweed Posted");

        return RedirectToAction("Index", "Home");
    }
}
