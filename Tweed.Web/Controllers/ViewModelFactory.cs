using System.Globalization;
using Humanizer;
using Tweed.Data.Entities;
using Tweed.Web.Views.Shared;

namespace Tweed.Web.Controllers;

public static class ViewModelFactory
{
    internal static TweedViewModel BuildTweedViewModel(Data.Entities.Tweed tweed, long likesCount, AppUser author,
        string currentUserId)
    {
        var humanizedCreatedAt = tweed.CreatedAt?.LocalDateTime.ToDateTimeUnspecified()
            .Humanize(true, null, CultureInfo.InvariantCulture);

        TweedViewModel viewModel = new()
        {
            Id = tweed.Id,
            Text = tweed.Text,
            CreatedAt = humanizedCreatedAt,
            AuthorId = tweed.AuthorId,
            LikesCount = likesCount,
            LikedByCurrentUser = tweed.Likes.Any(lb => lb.UserId == currentUserId),
            Author = author.UserName
        };
        return viewModel;
    }
}
