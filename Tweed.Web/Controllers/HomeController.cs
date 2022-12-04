using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tweed.Data;
using Tweed.Data.Entities;
using Tweed.Web.ViewModels;

namespace Tweed.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ITweedQueries _tweedQueries;
    private readonly UserManager<AppUser> _userManager;

    public HomeController(ITweedQueries tweedQueries, UserManager<AppUser> userManager)
    {
        _tweedQueries = tweedQueries;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var latestTweeds = await _tweedQueries.GetLatestTweeds();

        var currentUserId = _userManager.GetUserId(User);
        var tweeds = latestTweeds.Select(t => new TweedViewModel
        {
            Id = t.Id,
            Text = t.Text, CreatedAt = t.CreatedAt,
            AuthorId = t.AuthorId,
            Likes = t.LikedBy.Count,
            LikedByCurrentUser = t.LikedBy.Any(lb => lb.UserId == currentUserId)
        }).ToList();

        foreach (var tweed in tweeds)
            tweed.Author = (await _userManager.FindByIdAsync(tweed.AuthorId)).UserName;

        var viewModel = new IndexViewModel
        {
            Tweeds = tweeds
        };

        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
            { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
