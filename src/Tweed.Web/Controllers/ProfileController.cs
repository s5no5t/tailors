using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Tweed.Domain;
using Tweed.Domain.Model;
using Tweed.Web.Helper;
using Tweed.Web.Views.Profile;

namespace Tweed.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private const int PageSize = 100;
    private readonly IFollowsService _followsService;
    private readonly ITweedRepository _tweedRepository;
    private readonly IUserFollowsRepository _userFollowsRepository;
    private readonly UserManager<User> _userManager;
    private readonly IViewModelFactory _viewModelFactory;

    public ProfileController(ITweedRepository tweedRepository, UserManager<User> userManager,
        IViewModelFactory viewModelFactory, IUserFollowsRepository userFollowsRepository,
        IFollowsService followsService)
    {
        _tweedRepository = tweedRepository;
        _userManager = userManager;
        _viewModelFactory = viewModelFactory;
        _userFollowsRepository = userFollowsRepository;
        _followsService = followsService;
    }

    public async Task<IActionResult> Index(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        var userTweeds = await _tweedRepository.GetAllByAuthorId(userId, PageSize);

        var currentUserId = _userManager.GetUserId(User);
        var currentUserFollows = await _followsService.GetFollows(currentUserId!);

        var viewModel = new IndexViewModel(
            userId,
            user.UserName,
            await _viewModelFactory.BuildTweedViewModels(userTweeds),
            currentUserFollows.Any(f => f.LeaderId == user.Id),
            await _userFollowsRepository.GetFollowerCount(userId)
        );

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
        await _followsService.AddFollower(userId, currentUserId!, now);

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

        await _followsService.RemoveFollower(userId, currentUserId!);

        return RedirectToAction("Index", new
        {
            userId
        });
    }
}
