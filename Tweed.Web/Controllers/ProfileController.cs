using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Tweed.Data;
using Tweed.Data.Entities;
using Tweed.Web.Views.Profile;
using Tweed.Web.Views.Shared;

namespace Tweed.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IAppUserQueries _appUserQueries;
    private readonly ITweedQueries _tweedQueries;
    private readonly UserManager<AppUser> _userManager;

    public ProfileController(ITweedQueries tweedQueries, UserManager<AppUser> userManager,
        IAppUserQueries appUserQueries)
    {
        _tweedQueries = tweedQueries;
        _userManager = userManager;
        _appUserQueries = appUserQueries;
    }

    public async Task<IActionResult> Index(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        var userTweeds = await _tweedQueries.GetTweedsForUser(userId);
        var currentUser = await _userManager.GetUserAsync(User);

        List<TweedViewModel> tweedViewModels = new();
        foreach (var tweed in userTweeds)
        {
            var author = await _userManager.FindByIdAsync(tweed.AuthorId);
            int likesCount = await _tweedQueries.GetLikesCount(tweed.Id);
            var tweedViewModel =
                ViewModelFactory.BuildTweedViewModel(tweed, likesCount, author, currentUser.Id!);
            tweedViewModels.Add(tweedViewModel);
        }

        var viewModel = new IndexViewModel
        {
            UserId = userId,
            UserName = user.UserName,
            Tweeds = tweedViewModels,
            CurrentUserFollows = currentUser.Follows.Any(f => f.LeaderId == user.Id),
            FollowersCount = await _appUserQueries.GetFollowerCount(userId)
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Follow(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        var currentUserId = _userManager.GetUserId(User);
        if (userId == currentUserId)
            return BadRequest("Following yourself isn't allowed");

        var now = SystemClock.Instance.GetCurrentInstant().InUtc();
        await _appUserQueries.AddFollower(userId, currentUserId, now);

        return RedirectToAction("Index", new
        {
            userId
        });
    }

    [HttpPost]
    public async Task<IActionResult> Unfollow(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        var currentUserId = _userManager.GetUserId(User);

        await _appUserQueries.RemoveFollower(userId, currentUserId);

        return RedirectToAction("Index", new
        {
            userId
        });
    }
}
