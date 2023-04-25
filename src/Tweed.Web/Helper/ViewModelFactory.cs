using System.Globalization;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Tweed.Data;
using Tweed.Data.Entities;
using Tweed.Web.Views.Shared;

namespace Tweed.Web.Helper;

public interface IViewModelFactory
{
    Task<TweedViewModel> BuildTweedViewModel(Data.Entities.Tweed tweed);
}

public class ViewModelFactory : IViewModelFactory
{
    private readonly IIdentityUserQueries _identityUserQueries;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITweedUserQueries _tweedUserQueries;
    private readonly INotificationManager _notificationManager;
    private readonly ITweedQueries _tweedQueries;
    private readonly UserManager<TweedIdentityUser> _userManager;

    public ViewModelFactory(IIdentityUserQueries identityUserQueries, INotificationManager notificationManager,
        ITweedQueries tweedQueries, UserManager<TweedIdentityUser> userManager, IHttpContextAccessor httpContextAccessor, ITweedUserQueries tweedUserQueries)
    {
        _identityUserQueries = identityUserQueries;
        _notificationManager = notificationManager;
        _tweedQueries = tweedQueries;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _tweedUserQueries = tweedUserQueries;
    }

    public async Task<TweedViewModel> BuildTweedViewModel(Data.Entities.Tweed tweed)
    {
        var humanizedCreatedAt = tweed.CreatedAt?.LocalDateTime.ToDateTimeUnspecified()
            .Humanize(true, null, CultureInfo.InvariantCulture);
        var author = await _userManager.FindByIdAsync(tweed.AuthorId);
        var likesCount = await _tweedQueries.GetLikesCount(tweed.Id!);
        var currentIdentityUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext!.User);
        var currentTweedUser = await _tweedUserQueries.FindByIdentityUserId(currentIdentityUser.Id!);

        TweedViewModel viewModel = new()
        {
            Id = tweed.Id,
            Text = tweed.Text,
            CreatedAt = humanizedCreatedAt,
            AuthorId = tweed.AuthorId,
            LikesCount = likesCount,
            LikedByCurrentUser = currentTweedUser.Likes.Any(lb => lb.TweedId == tweed.Id),
            Author = author.UserName
        };
        return viewModel;
    }
}
