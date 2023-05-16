using System.Globalization;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Tweed.Domain;
using Tweed.Domain.Model;
using Tweed.Web.Views.Shared;

namespace Tweed.Web.Helper;

public interface IViewModelFactory
{
    Task<TweedViewModel> BuildTweedViewModel(Domain.Model.Tweed tweed);
    Task<List<TweedViewModel>> BuildTweedViewModels(List<Domain.Model.Tweed> tweeds);
}

public class ViewModelFactory : IViewModelFactory
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILikesService _likesService;
    private readonly ITweedLikesRepository _tweedLikesRepository;
    private readonly UserManager<User> _userManager;

    public ViewModelFactory(ITweedLikesRepository tweedLikesRepository, ILikesService likesService,
        UserManager<User> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _tweedLikesRepository = tweedLikesRepository;
        _likesService = likesService;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TweedViewModel> BuildTweedViewModel(Domain.Model.Tweed tweed)
    {
        var humanizedCreatedAt = tweed.CreatedAt?.LocalDateTime.ToDateTimeUnspecified()
            .Humanize(true, null, CultureInfo.InvariantCulture);
        var author = await _userManager.FindByIdAsync(tweed.AuthorId!);
        var likesCount = await _tweedLikesRepository.GetLikesCounter(tweed.Id!);

        var currentUserId = _userManager.GetUserId(_httpContextAccessor.HttpContext!.User);
        var currentUserLikesTweed =
            await _likesService.DoesUserLikeTweed(tweed.Id!, currentUserId!);

        TweedViewModel viewModel = new()
        {
            Id = tweed.Id,
            Text = tweed.Text,
            CreatedAt = humanizedCreatedAt,
            AuthorId = tweed.AuthorId,
            LikesCount = likesCount,
            LikedByCurrentUser = currentUserLikesTweed,
            Author = author!.UserName
        };
        return viewModel;
    }

    public async Task<List<TweedViewModel>> BuildTweedViewModels(List<Domain.Model.Tweed> tweeds)
    {
        List<TweedViewModel> tweedViewModels = new();
        foreach (var tweed in tweeds)
        {
            var tweedViewModel = await BuildTweedViewModel(tweed);
            tweedViewModels.Add(tweedViewModel);
        }

        return tweedViewModels;
    }
}
