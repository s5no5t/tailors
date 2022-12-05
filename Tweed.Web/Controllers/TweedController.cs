using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Tweed.Data;
using Tweed.Data.Entities;
using Tweed.Web.Helper;
using Tweed.Web.Views.Shared;
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

    [HttpPost]
    public async Task<IActionResult> Like(string tweedId)
    {
        var tweed = await _tweedQueries.GetById(tweedId);
        if (tweed == null)
            return NotFound();

        var currentUserId = _userManager.GetUserId(User);
        var now = SystemClock.Instance.GetCurrentInstant().InUtc();
        await _tweedQueries.AddLike(tweedId, currentUserId, now);

        TweedViewModel viewModel = new()
        {
            Id = tweed.Id,
            Text = tweed.Text, CreatedAt = tweed.CreatedAt,
            AuthorId = tweed.AuthorId,
            Likes = tweed.LikedBy.Count,
            LikedByCurrentUser = tweed.LikedBy.Any(lb => lb.UserId == currentUserId)
        };

        return PartialView("_Tweed", viewModel);
    }

    public async Task<IActionResult> Unlike(string tweedId)
    {
        var tweed = await _tweedQueries.GetById(tweedId);
        if (tweed == null)
            return NotFound();

        var currentUserId = _userManager.GetUserId(User);
        await _tweedQueries.RemoveLike(tweedId, currentUserId);

        TweedViewModel viewModel = new()
        {
            Id = tweed.Id,
            Text = tweed.Text, CreatedAt = tweed.CreatedAt,
            AuthorId = tweed.AuthorId,
            Likes = tweed.LikedBy.Count,
            LikedByCurrentUser = tweed.LikedBy.Any(lb => lb.UserId == currentUserId)
        };

        return PartialView("_Tweed", viewModel);
    }
}
