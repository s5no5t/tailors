using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Tweed.User;
using Tweed.Like.Domain;
using Tweed.Thread.Domain;
using Tweed.User.Domain;
using Tweed.Web.Helper;
using Tweed.Web.Views.Tweed;

namespace Tweed.Web.Controllers;

[Authorize]
public class TweedController : Controller
{
    private readonly INotificationManager _notificationManager;
    private readonly IThreadOfTweedsUseCase _threadOfTweedsUseCase;
    private readonly ITweedRepository _tweedRepository;
    private readonly ITweedViewModelFactory _tweedViewModelFactory;
    private readonly UserManager<AppUser> _userManager;

    public TweedController(ITweedRepository tweedRepository,
        UserManager<AppUser> userManager,
        INotificationManager notificationManager,
        IThreadOfTweedsUseCase threadOfTweedsUseCase,
        ITweedViewModelFactory tweedViewModelFactory)
    {
        _tweedRepository = tweedRepository;
        _userManager = userManager;
        _notificationManager = notificationManager;
        _threadOfTweedsUseCase = threadOfTweedsUseCase;
        _tweedViewModelFactory = tweedViewModelFactory;
    }

    [HttpGet("Tweed/{tweedId}")]
    public async Task<ActionResult> ShowThreadForTweed(string tweedId)
    {
        var decodedTweedId =
            HttpUtility.UrlDecode(tweedId); // ASP.NET Core doesn't auto-decode parameters

        var threadTweeds = await _threadOfTweedsUseCase.GetThreadTweedsForTweed(decodedTweedId);
        if (threadTweeds.IsFailed)
            return NotFound();

        ShowThreadForTweedViewModel viewModel = new()
        {
            Tweeds = await _tweedViewModelFactory.Create(threadTweeds.Value),
            CreateReplyTweed = new CreateReplyTweedViewModel
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
    public async Task<IActionResult> Create(CreateTweedViewModel viewModel,
        [FromServices] ICreateTweedUseCase createTweedUseCase)
    {
        if (!ModelState.IsValid) return PartialView("_CreateTweed", viewModel);

        var currentUserId = _userManager.GetUserId(User);
        var now = SystemClock.Instance.GetCurrentInstant().InUtc();

        await createTweedUseCase.CreateRootTweed(currentUserId!, viewModel.Text, now);

        _notificationManager.AppendSuccess("Tweed Posted");

        return RedirectToAction("Index", "Feed");
    }

    [HttpPost]
    public async Task<IActionResult> CreateReply(CreateReplyTweedViewModel viewModel,
        [FromServices] ICreateTweedUseCase createTweedUseCase)
    {
        if (!ModelState.IsValid) return PartialView("_CreateReplyTweed", viewModel);

        if (viewModel.ParentTweedId is null)
            return BadRequest();

        var parentTweed = await _tweedRepository.GetById(viewModel.ParentTweedId);
        if (parentTweed is null)
            return BadRequest();

        var currentUserId = _userManager.GetUserId(User);
        var now = SystemClock.Instance.GetCurrentInstant().InUtc();

        var tweed = await createTweedUseCase.CreateReplyTweed(currentUserId!, viewModel.Text, now,
            viewModel.ParentTweedId);

        _notificationManager.AppendSuccess("Reply Posted");

        return RedirectToAction("Index", "Feed");
    }

    [HttpPost]
    public async Task<IActionResult> Like(string tweedId, [FromServices] ILikeTweedUseCase likeTweedUseCase)
    {
        var tweed = await _tweedRepository.GetById(tweedId);
        if (tweed == null)
            return NotFound();

        var currentUserId = _userManager.GetUserId(User);
        var now = SystemClock.Instance.GetCurrentInstant().InUtc();
        await likeTweedUseCase.AddLike(tweedId, currentUserId!, now);

        var viewModel = await _tweedViewModelFactory.Create(tweed);
        return PartialView("_Tweed", viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Unlike(string tweedId, [FromServices] ILikeTweedUseCase likeTweedUseCase)
    {
        var tweed = await _tweedRepository.GetById(tweedId);
        if (tweed == null)
            return NotFound();

        var currentUserId = _userManager.GetUserId(User);
        await likeTweedUseCase.RemoveLike(tweedId, currentUserId!);

        var viewModel = await _tweedViewModelFactory.Create(tweed);
        return PartialView("_Tweed", viewModel);
    }
}
