using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tweed.Data;
using Tweed.Data.Entities;
using Tweed.Web.ViewModels;

namespace Tweed.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly ITweedQueries _tweedQueries;
    private readonly UserManager<AppUser> _userManager;

    public ProfileController(ITweedQueries tweedQueries, UserManager<AppUser> userManager)
    {
        _tweedQueries = tweedQueries;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        var userTweeds = await _tweedQueries.GetTweedsForUser(userId);

        var currentUserId = _userManager.GetUserId(User);
        var tweeds = userTweeds.Select(t => new TweedViewModel
        {
            Id = t.Id,
            Text = t.Text, CreatedAt = t.CreatedAt,
            AuthorId = t.AuthorId,
            Likes = t.LikedBy.Count,
            LikedByCurrentUser = t.LikedBy.Any(lb => lb.UserId == currentUserId)
        }).ToList();

        foreach (var tweed in tweeds)
            tweed.Author = (await _userManager.FindByIdAsync(tweed.AuthorId)).UserName;

        var viewModel = new ProfileViewModel
        {
            UserName = user.UserName,
            Tweeds = tweeds
        };

        return View(viewModel);
    }
}
