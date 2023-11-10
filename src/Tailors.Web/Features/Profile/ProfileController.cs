using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tailors.Domain.TweedAggregate;
using Tailors.Domain.UserAggregate;
using Tailors.Domain.UserFollowsAggregate;
using Tailors.Web.Helper;

namespace Tailors.Web.Features.Profile;

[Authorize]
public class ProfileController : Controller
{
    private const int PageSize = 100;
    private readonly FollowUserUseCase _followUserUseCase;
    private readonly ITweedRepository _tweedRepository;
    private readonly ITweedViewModelFactory _tweedViewModelFactory;
    private readonly IUserFollowsRepository _userFollowsRepository;
    private readonly UserManager<AppUser> _userManager;

    public ProfileController(ITweedRepository tweedRepository, UserManager<AppUser> userManager,
        ITweedViewModelFactory tweedViewModelFactory, IUserFollowsRepository userFollowsRepository,
        FollowUserUseCase followUserUseCase)
    {
        _tweedRepository = tweedRepository;
        _userManager = userManager;
        _tweedViewModelFactory = tweedViewModelFactory;
        _userFollowsRepository = userFollowsRepository;
        _followUserUseCase = followUserUseCase;
    }

    public async Task<IActionResult> Index(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        var userTweeds = await _tweedRepository.GetAllByAuthorId(userId, PageSize);

        var currentUserId = _userManager.GetUserId(User)!;
        var currentUserFollows = await _followUserUseCase.GetFollows(currentUserId!);

        var viewModel = new IndexViewModel(
            userId,
            user.UserName,
            await _tweedViewModelFactory.Create(userTweeds, currentUserId),
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

        var now = DateTime.UtcNow;
        await _followUserUseCase.AddFollower(userId, currentUserId!, now);

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

        await _followUserUseCase.RemoveFollower(userId, currentUserId!);

        return RedirectToAction("Index", new
        {
            userId
        });
    }
}
