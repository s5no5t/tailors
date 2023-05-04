using System.Globalization;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Tweed.Data.Domain;
using Tweed.Data.Model;
using Tweed.Web.Views.Shared;

namespace Tweed.Web.Helper;

public interface IViewModelFactory
{
    Task<TweedViewModel> BuildTweedViewModel(Data.Model.Tweed tweed);
}

public class ViewModelFactory : IViewModelFactory
{
    private readonly IAppUserLikesQueries _appUserLikesQueries;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITweedQueries _tweedQueries;
    private readonly UserManager<AppUser> _userManager;

    public ViewModelFactory(
        IAppUserLikesQueries appUserLikesQueries,
        ITweedQueries tweedQueries, UserManager<AppUser> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _appUserLikesQueries = appUserLikesQueries;
        _tweedQueries = tweedQueries;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TweedViewModel> BuildTweedViewModel(Data.Model.Tweed tweed)
    {
        var humanizedCreatedAt = tweed.CreatedAt?.LocalDateTime.ToDateTimeUnspecified()
            .Humanize(true, null, CultureInfo.InvariantCulture);
        var author = await _userManager.FindByIdAsync(tweed.AuthorId!);
        var likesCount = await _tweedQueries.GetLikesCount(tweed.Id!);

        var currentUserId = _userManager.GetUserId(_httpContextAccessor.HttpContext!.User);
        var currentUserLikes = await _appUserLikesQueries.GetLikes(currentUserId!);

        TweedViewModel viewModel = new()
        {
            Id = tweed.Id,
            Text = tweed.Text,
            CreatedAt = humanizedCreatedAt,
            AuthorId = tweed.AuthorId,
            LikesCount = likesCount,
            LikedByCurrentUser = currentUserLikes.Any(lb => lb.TweedId == tweed.Id),
            Author = author!.UserName
        };
        return viewModel;
    }
}