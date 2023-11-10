using System.Globalization;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Tailors.Domain.TweedAggregate;
using Tailors.Domain.UserAggregate;
using Tailors.Domain.UserLikesAggregate;
using Tailors.Web.Features.Shared;

namespace Tailors.Web.Helper;

public class TweedViewModelFactory
{
    private readonly LikeTweedUseCase _likeTweedUseCase;
    private readonly IUserLikesRepository _userLikesRepository;
    private readonly UserManager<AppUser> _userManager;

    public TweedViewModelFactory(IUserLikesRepository userLikesRepository,
        LikeTweedUseCase likeTweedUseCase,
        UserManager<AppUser> userManager)
    {
        _userLikesRepository = userLikesRepository;
        _likeTweedUseCase = likeTweedUseCase;
        _userManager = userManager;
    }

    public async Task<TweedViewModel> Create(Tweed tweed, string currentUserId, bool isCurrent = false)
    {
        var humanizedCreatedAt = tweed.CreatedAt.Humanize(true, null, CultureInfo.InvariantCulture);
        var author = await _userManager.FindByIdAsync(tweed.AuthorId);
        var likesCount = await _userLikesRepository.GetLikesCounter(tweed.Id!);
        var currentUserLikesTweed =
            await _likeTweedUseCase.DoesUserLikeTweed(tweed.Id!, currentUserId!);

        TweedViewModel viewModel = new()
        {
            Id = tweed.Id,
            Text = tweed.Text,
            CreatedAt = humanizedCreatedAt,
            AuthorId = tweed.AuthorId,
            LikesCount = likesCount,
            LikedByCurrentUser = currentUserLikesTweed,
            Author = author!.UserName,
            IsCurrentTweed = isCurrent
        };
        return viewModel;
    }

    public async Task<List<TweedViewModel>> Create(List<Tweed> tweeds, string currentUserId,
        string currentTweedId = "none")
    {
        List<TweedViewModel> tweedViewModels = new();
        foreach (var tweed in tweeds)
        {
            var tweedViewModel = await Create(tweed, currentUserId, tweed.Id == currentTweedId);
            tweedViewModels.Add(tweedViewModel);
        }

        return tweedViewModels;
    }
}
