using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tailors.Domain.TweedAggregate;
using Tailors.Domain.UserAggregate;
using Tailors.Domain.UserFollowsAggregate;
using Tailors.Web.Helper;

namespace Tailors.Web.Features.Profile;

[Authorize]
public class ProfileController(
    ITweedRepository tweedRepository,
    IUserRepository userRepository,
    TweedViewModelFactory tweedViewModelFactory,
    IUserFollowsRepository userFollowsRepository,
    FollowUserUseCase followUserUseCase)
    : Controller
{
    private const int PageSize = 100;

    public async Task<IActionResult> Index(string userId)
    {
        var user = await userRepository.GetById(userId);
        if (user == null)
            return NotFound();

        var userTweeds = await tweedRepository.GetAllByAuthorId(userId, PageSize);

        var currentUserId = User.GetId();
        var currentUserFollows = await followUserUseCase.GetFollows(currentUserId);

        var viewModel = new IndexViewModel(
            userId,
            user.UserName,
            await tweedViewModelFactory.Create(userTweeds, currentUserId),
            currentUserFollows.Any(f => f.LeaderId == user.Id),
            await userFollowsRepository.GetFollowerCount(userId)
        );

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Follow(string userId)
    {
        var user = await userRepository.GetById(userId);
        if (user == null)
            return NotFound();

        var currentUserId = User.GetId();
        if (userId == currentUserId)
            return BadRequest("Following yourself isn't allowed");

        var now = DateTime.UtcNow;
        await followUserUseCase.AddFollower(userId, currentUserId!, now);

        return RedirectToAction("Index", new
        {
            userId
        });
    }

    [HttpPost]
    public async Task<IActionResult> Unfollow(string userId)
    {
        var user = await userRepository.GetById(userId);
        if (user == null)
            return NotFound();

        var currentUserId = User.GetId();

        await followUserUseCase.RemoveFollower(userId, currentUserId!);

        return RedirectToAction("Index", new
        {
            userId
        });
    }
}
