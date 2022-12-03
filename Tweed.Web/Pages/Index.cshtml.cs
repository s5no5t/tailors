using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tweed.Data;
using Tweed.Data.Entities;
using Tweed.Web.Pages.Shared;

namespace Tweed.Web.Pages;

[Authorize]
public class IndexPageModel : PageModel
{
    private readonly ITweedQueries _tweedQueries;
    private readonly UserManager<AppUser> _userManager;

    public IndexPageModel(ITweedQueries tweedQueries, UserManager<AppUser> userManager)
    {
        _tweedQueries = tweedQueries;
        _userManager = userManager;
    }

    public List<TweedViewModel> Tweeds { get; } = new();

    public async Task OnGetAsync()
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

        Tweeds.AddRange(tweeds);
    }
}
