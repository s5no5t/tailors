using System.Globalization;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Tweed.Domain.Model;
using Tweed.Like.Domain;
using Tweed.Web.Views.Shared;

namespace Tweed.Web.Helper;

public interface ITweedViewModelFactory
{
    Task<TweedViewModel> Create(TheTweed theTweed);
    Task<List<TweedViewModel>> Create(List<TheTweed> tweeds);
}

public class TweedViewModelFactory : ITweedViewModelFactory
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILikeTweedUseCase _likeTweedUseCase;
    private readonly ITweedLikesRepository _tweedLikesRepository;
    private readonly UserManager<User> _userManager;

    public TweedViewModelFactory(ITweedLikesRepository tweedLikesRepository, ILikeTweedUseCase likeTweedUseCase,
        UserManager<User> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _tweedLikesRepository = tweedLikesRepository;
        _likeTweedUseCase = likeTweedUseCase;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TweedViewModel> Create(TheTweed theTweed)
    {
        var humanizedCreatedAt = theTweed.CreatedAt?.LocalDateTime.ToDateTimeUnspecified()
            .Humanize(true, null, CultureInfo.InvariantCulture);
        var author = await _userManager.FindByIdAsync(theTweed.AuthorId!);
        var likesCount = await _tweedLikesRepository.GetLikesCounter(theTweed.Id!);

        var currentUserId = _userManager.GetUserId(_httpContextAccessor.HttpContext!.User);
        var currentUserLikesTweed =
            await _likeTweedUseCase.DoesUserLikeTweed(theTweed.Id!, currentUserId!);

        TweedViewModel viewModel = new()
        {
            Id = theTweed.Id,
            Text = theTweed.Text,
            CreatedAt = humanizedCreatedAt,
            AuthorId = theTweed.AuthorId,
            LikesCount = likesCount,
            LikedByCurrentUser = currentUserLikesTweed,
            Author = author!.UserName
        };
        return viewModel;
    }

    public async Task<List<TweedViewModel>> Create(List<TheTweed> tweeds)
    {
        List<TweedViewModel> tweedViewModels = new();
        foreach (var tweed in tweeds)
        {
            var tweedViewModel = await Create(tweed);
            tweedViewModels.Add(tweedViewModel);
        }

        return tweedViewModels;
    }
}