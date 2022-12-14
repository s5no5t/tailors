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
    private readonly IAppUserQueries _appUserQueries;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly INotificationManager _notificationManager;
    private readonly ITweedQueries _tweedQueries;
    private readonly UserManager<AppUser> _userManager;

    public ViewModelFactory(IAppUserQueries appUserQueries, INotificationManager notificationManager,
        ITweedQueries tweedQueries, UserManager<AppUser> userManager, IHttpContextAccessor httpContextAccessor)
    {
        _appUserQueries = appUserQueries;
        _notificationManager = notificationManager;
        _tweedQueries = tweedQueries;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TweedViewModel> BuildTweedViewModel(Data.Entities.Tweed tweed)
    {
        var humanizedCreatedAt = tweed.CreatedAt?.LocalDateTime.ToDateTimeUnspecified()
            .Humanize(true, null, CultureInfo.InvariantCulture);
        var author = await _userManager.FindByIdAsync(tweed.AuthorId);
        var likesCount = await _tweedQueries.GetLikesCount(tweed.Id);
        var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);

        TweedViewModel viewModel = new()
        {
            Id = tweed.Id,
            Text = tweed.Text,
            CreatedAt = humanizedCreatedAt,
            AuthorId = tweed.AuthorId,
            LikesCount = likesCount,
            LikedByCurrentUser = currentUser.Likes.Any(lb => lb.TweedId == tweed.Id),
            Author = author.UserName
        };
        return viewModel;
    }
}
