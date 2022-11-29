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

        var tweeds = await Task.WhenAll(latestTweeds.Select(async l => new Tweed
        {
            Id = l.Id,
            Text = l.Text, CreatedAt = l.CreatedAt,
            Author = l.AuthorId != null
                ? (await _userManager.FindByIdAsync(l.AuthorId)).UserName
                : null,
            Likes = l.Likes
        }));

        Tweeds.AddRange(tweeds);
    }

    private string GetLinkId(string ravenDbId)
    {
        var splits = ravenDbId.Split('/');
        if (splits.Length != 2)
            throw new ArgumentException("Can't parse ID");
        return splits[1];
    }

    public class Tweed
    {
        public string? Author { get; set; }
        public string? Text { get; set; }
        public ZonedDateTime? CreatedAt { get; set; }
        public string Id { get; set; }
        public int? Likes { get; set; }
    }
}
