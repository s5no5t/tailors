using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NodaTime;
using Tweed.Data;
using Tweed.Data.Entities;

namespace Tweed.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ITweedQueries _tweedQueries;
    private readonly UserManager<AppUser> _userManager;

    public IndexModel(ITweedQueries tweedQueries, UserManager<AppUser> userManager)
    {
        _tweedQueries = tweedQueries;
        _userManager = userManager;
    }

    public List<Tweed> Tweeds { get; } = new();

    public async Task OnGetAsync()
    {
        var latestTweeds = await _tweedQueries.GetLatestTweeds();

        var tweeds = latestTweeds.Select(l => new Tweed
        {
            Id = l.Id,
            Text = l.Text, CreatedAt = l.CreatedAt,
            AuthorId = l.AuthorId,
            Likes = l.LikedBy.Count
        }).ToList();

        foreach (var tweed in tweeds)
            tweed.Author = (await _userManager.FindByIdAsync(tweed.AuthorId)).UserName;

        Tweeds.AddRange(tweeds);
    }

    public class Tweed
    {
        public string? AuthorId { get; set; }
        public string? Author { get; set; }
        public string? Text { get; set; }
        public ZonedDateTime? CreatedAt { get; set; }
        public string? Id { get; set; }
        public int? Likes { get; set; }
    }
}
