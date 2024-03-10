using System.Globalization;
using Humanizer;
using Tailors.Domain.TweedAggregate;
using Tailors.Domain.UserAggregate;
using Tailors.Domain.UserLikesAggregate;
using Tailors.Web.Features.Tweed;

namespace Tailors.Web.Helper;

public class TweedViewModelFactory(
    IUserLikesRepository userLikesRepository,
    LikeTweedUseCase likeTweedUseCase,
    IUserRepository userRepository)
{
    public async Task<TweedViewModel> Create(Tweed tweed, string currentUserId, bool isCurrent = false)
    {
        var humanizedCreatedAt = tweed.CreatedAt.Humanize(true, null, CultureInfo.InvariantCulture);
        var author = await userRepository.GetById(tweed.AuthorId);
        var likesCount = await userLikesRepository.GetLikesCounter(tweed.Id!);
        var currentUserLikesTweed =
            await likeTweedUseCase.DoesUserLikeTweed(tweed.Id!, currentUserId);

        TweedViewModel viewModel = new()
        {
            Id = tweed.Id,
            Text = tweed.Text,
            CreatedAt = humanizedCreatedAt,
            AuthorId = tweed.AuthorId,
            LikesCount = likesCount,
            LikedByCurrentUser = currentUserLikesTweed,
            Author = author?.UserName,
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
