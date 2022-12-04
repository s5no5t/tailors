using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NodaTime;
using Tweed.Data;
using Tweed.Data.Entities;
using Tweed.Web.Controllers;
using Tweed.Web.Models;
using Tweed.Web.Test.TestHelper;
using Xunit;

namespace Tweed.Web.Test.Controllers;

public class HomeControllerTest
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;

    public HomeControllerTest()
    {
        _userManagerMock = UserManagerMockHelper.MockUserManager<AppUser>();
    }

    [Fact]
    public void RequiresAuthorization()
    {
        var authorizeAttributeValue =
            Attribute.GetCustomAttribute(typeof(HomeController), typeof(AuthorizeAttribute));
        Assert.NotNull(authorizeAttributeValue);
    }

    [Fact]
    public async Task Index_ShouldLoadLatestTweeds()
    {
        var tweedQueriesMock = new Mock<ITweedQueries>();
        var indexModel = new HomeController(tweedQueriesMock.Object, _userManagerMock.Object);

        await indexModel.Index();

        tweedQueriesMock.Verify(t => t.GetLatestTweeds());
    }

    [Fact]
    public async Task Index_ShouldMarkTweedsWrittenByCurrentUser()
    {
        var tweedQueriesMock = new Mock<ITweedQueries>();

        var fixedZonedDateTime = new ZonedDateTime(new LocalDateTime(2022, 11, 18, 15, 20),
            DateTimeZone.Utc, new Offset());
        var tweed = new Data.Entities.Tweed
        {
            LikedBy = new List<LikedBy>
                { new() { UserId = "user1", LikedAt = fixedZonedDateTime } },
            AuthorId = "user2"
        };
        tweedQueriesMock.Setup(t => t.GetLatestTweeds()).ReturnsAsync(new[] { tweed });
        _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");
        var appUser = new AppUser
        {
            UserName = "User 2"
        };
        _userManagerMock.Setup(u => u.FindByIdAsync("user2")).ReturnsAsync(appUser);
        var indexModel = new HomeController(tweedQueriesMock.Object, _userManagerMock.Object);

        var result = await indexModel.Index();

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<IndexViewModel>(resultAsView.Model);
        var viewModel = (IndexViewModel)resultAsView.Model!;
        Assert.True(viewModel.Tweeds[0].LikedByCurrentUser);
    }
}
