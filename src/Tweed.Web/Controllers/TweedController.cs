using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Tweed.Domain;
using Tweed.Domain.Model;
using Tweed.Web.Helper;
using Tweed.Web.Views.Shared;
using Tweed.Web.Views.Tweed;

namespace Tweed.Web.Controllers;

[Authorize]
public class TweedController : Controller
{
    private readonly INotificationManager _notificationManager;
    private readonly ITweedThreadService _threadService;
    private readonly ITweedLikesService _tweedLikesService;
    private readonly ITweedService _tweedService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IViewModelFactory _viewModelFactory;

    public TweedController(ITweedService tweedService, UserManager<AppUser> userManager,
        INotificationManager notificationManager, ITweedLikesService tweedLikesService,
        IViewModelFactory viewModelFactory, ITweedThreadService threadService)
    {
        _tweedService = tweedService;
        _userManager = userManager;
        _notificationManager = notificationManager;
        _tweedLikesService = tweedLikesService;
        _viewModelFactory = viewModelFactory;
        _threadService = threadService;
    }

    [HttpGet("Tweed/{tweedId}")]
    public async Task<ActionResult> GetById(string tweedId)
    {
        var decodedTweedId =
            HttpUtility.UrlDecode(tweedId); // ASP.NET Core doesn't auto-decode parameters

        var tweed = await _tweedService.GetById(decodedTweedId);
        if (tweed == null)
            return NotFound();

        var tweedViewModel = await _viewModelFactory.BuildTweedViewModel(tweed);
        GetByIdViewModel viewModel = new()
        {
            LeadingTweeds = new List<TweedViewModel>(),
            Tweed = tweedViewModel,
            CreateTweed = new CreateReplyTweedViewModel
            {
                ParentTweedId = decodedTweedId
            }
        };
        return View(viewModel);
    }

    [HttpGet("Tweed/Create")]
    public IActionResult Create()
    {
        CreateViewModel viewModel = new();
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTweedViewModel viewModel)
    {
        if (!ModelState.IsValid) return PartialView("_CreateTweed", viewModel);

        var currentUserId = _userManager.GetUserId(User);
        var now = SystemClock.Instance.GetCurrentInstant().InUtc();

        Domain.Model.Tweed tweed = new()
        {
            CreatedAt = now,
            AuthorId = currentUserId,
            Text = viewModel.Text
        };
        await _tweedService.CreateTweed(tweed);

        _notificationManager.AppendSuccess("Tweed Posted");

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> CreateReply(CreateReplyTweedViewModel viewModel)
    {
        if (!ModelState.IsValid) return PartialView("_CreateReplyTweed", viewModel);

        if (viewModel.ParentTweedId is null)
            return BadRequest();

        var parentTweed = await _tweedService.GetById(viewModel.ParentTweedId);
        if (parentTweed is null)
            return BadRequest();

        var currentUserId = _userManager.GetUserId(User);
        var now = SystemClock.Instance.GetCurrentInstant().InUtc();

        Domain.Model.Tweed tweed = new()
        {
            ParentTweedId = viewModel.ParentTweedId,
            CreatedAt = now,
            AuthorId = currentUserId,
            Text = viewModel.Text,
        };
        await _tweedService.CreateTweed(tweed);

        _notificationManager.AppendSuccess("Reply Posted");

        return RedirectToAction("GetById", new
        {
            tweedId = tweed.Id
        });
    }

    [HttpPost]
    public async Task<IActionResult> Like(string tweedId)
    {
        var tweed = await _tweedService.GetById(tweedId);
        if (tweed == null)
            return NotFound();

        var currentUserId = _userManager.GetUserId(User);
        var now = SystemClock.Instance.GetCurrentInstant().InUtc();
        await _tweedLikesService.AddLike(tweedId, currentUserId!, now);

        var viewModel = await _viewModelFactory.BuildTweedViewModel(tweed);
        return PartialView("_Tweed", viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Unlike(string tweedId)
    {
        var tweed = await _tweedService.GetById(tweedId);
        if (tweed == null)
            return NotFound();

        var currentUserId = _userManager.GetUserId(User);
        await _tweedLikesService.RemoveLike(tweedId, currentUserId!);

        var viewModel = await _viewModelFactory.BuildTweedViewModel(tweed);
        return PartialView("_Tweed", viewModel);
    }
}
