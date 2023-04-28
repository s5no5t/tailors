using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Tweed.Data;
using Tweed.Data.Model;
using Tweed.Web.Helper;
using Tweed.Web.Views.Tweed;

namespace Tweed.Web.Controllers;

[Authorize]
public class TweedController : Controller
{
    private readonly IAppUserLikesQueries _appUserLikesQueries;
    private readonly IAppUserQueries _appUserQueries;
    private readonly INotificationManager _notificationManager;
    private readonly ITweedQueries _tweedQueries;
    private readonly UserManager<AppUser> _userManager;
    private readonly IViewModelFactory _viewModelFactory;

    public TweedController(ITweedQueries tweedQueries, UserManager<AppUser> userManager,
        INotificationManager notificationManager, IAppUserQueries appUserQueries,
        IAppUserLikesQueries appUserLikesQueries, IViewModelFactory viewModelFactory)
    {
        _tweedQueries = tweedQueries;
        _userManager = userManager;
        _notificationManager = notificationManager;
        _appUserQueries = appUserQueries;
        _appUserLikesQueries = appUserLikesQueries;
        _viewModelFactory = viewModelFactory;
    }

    [HttpGet("Tweed/{tweedId}")]
    public async Task<ActionResult> GetById(string tweedId)
    {
        var decodedId =
            HttpUtility.UrlDecode(tweedId); // ASP.NET Core doesn't auto-decode parameters

        var tweed = await _tweedQueries.GetById(decodedId);
        if (tweed == null)
            return NotFound();

        var tweedViewModel = await _viewModelFactory.BuildTweedViewModel(tweed);
        GetByIdViewModel viewModel = new(tweedViewModel);
        return View(viewModel);
    }

    [HttpGet("Tweed/Create")]
    public IActionResult Create()
    {
        CreateViewModel viewModel = new();
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateViewModel viewModel)
    {
        if (!ModelState.IsValid) return View(viewModel);

        var currentUserId = _userManager.GetUserId(User);
        var now = SystemClock.Instance.GetCurrentInstant().InUtc();

        await _tweedQueries.StoreTweed(viewModel.Text, currentUserId!, now);

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
        await _appUserLikesQueries.AddLike(tweedId, currentUserId!, now);

        var viewModel = await _viewModelFactory.BuildTweedViewModel(tweed);
        return PartialView("_Tweed", viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Unlike(string tweedId)
    {
        var tweed = await _tweedQueries.GetById(tweedId);
        if (tweed == null)
            return NotFound();

        var currentUserId = _userManager.GetUserId(User);
        await _appUserLikesQueries.RemoveLike(tweedId, currentUserId!);

        var viewModel = await _viewModelFactory.BuildTweedViewModel(tweed);
        return PartialView("_Tweed", viewModel);
    }
}
