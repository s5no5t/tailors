using Tweed.Data.Entities;
using Tweed.Web.Views.Shared;

namespace Tweed.Web.Controllers;

public static class ViewModelFactory
{
    internal static TweedViewModel BuildTweedViewModel(Data.Entities.Tweed tweed, AppUser author,
        string currentUserId)
    {
        TweedViewModel viewModel = new()
        {
            Id = tweed.Id,
            Text = tweed.Text, CreatedAt = tweed.CreatedAt,
            AuthorId = tweed.AuthorId,
            LikesCount = tweed.Likes.Count,
            LikedByCurrentUser = tweed.Likes.Any(lb => lb.UserId == currentUserId),
            Author = author.UserName
        };
        return viewModel;
    }
}