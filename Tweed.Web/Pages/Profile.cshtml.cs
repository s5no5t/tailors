using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tweed.Data;
using Tweed.Data.Entities;
using Tweed.Web.Pages.Shared;

namespace Tweed.Web.Pages;

[Authorize]
public class ProfilePageModel : PageModel
{
    private readonly ITweedQueries _tweedQueries;
    private readonly UserManager<AppUser> _userManager;

    public ProfilePageModel(ITweedQueries tweedQueries, UserManager<AppUser> userManager)
    {
        _tweedQueries = tweedQueries;
        _userManager = userManager;
    }

    public List<TweedViewModel> Tweeds { get; } = new();
    [FromQuery(Name = "user-id")] public string? UserId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (UserId == null)
            return NotFound();

        var user = await _userManager.FindByIdAsync(UserId);
        if (user == null)
            return NotFound();

        var userTweeds = await _tweedQueries.GetTweedsForUser(UserId);

        var tweeds = userTweeds.Select(l => new TweedViewModel
        {
            Id = l.Id,
            Text = l.Text, CreatedAt = l.CreatedAt,
            AuthorId = l.AuthorId,
            Likes = l.LikedBy.Count
        }).ToList();

        foreach (var tweed in tweeds)
            tweed.Author = (await _userManager.FindByIdAsync(tweed.AuthorId)).UserName;

        Tweeds.AddRange(tweeds);

        return Page();
    }
}
