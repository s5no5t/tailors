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
using Tweed.Web.Test.TestHelper;
using Tweed.Web.Views.Home;
using Xunit;

namespace Tweed.Web.Test.Controllers;

public class HomeControllerTest
{
    private readonly HomeController _homeController;
    private readonly Mock<ITweedQueries> _tweedQueriesMock;
    private readonly Mock<UserManager<AppUser>> _userManagerMock;

    public HomeControllerTest()
    {
        var currentUserPrincipal = ControllerTestHelper.BuildPrincipal();
        _userManagerMock = UserManagerMockHelper.MockUserManager<AppUser>();
        _userManagerMock.Setup(u => u.GetUserId(currentUserPrincipal)).Returns("user1");
        _tweedQueriesMock = new Mock<ITweedQueries>();
        _tweedQueriesMock.Setup(t => t.GetLatestTweeds())
            .ReturnsAsync(new List<Data.Entities.Tweed>());
        _homeController = new HomeController(_tweedQueriesMock.Object, _userManagerMock.Object)
        {
            ControllerContext = ControllerTestHelper.BuildControllerContext(currentUserPrincipal)
        };
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
        await _homeController.Index();

        _tweedQueriesMock.Verify(t => t.GetLatestTweeds());
    }

    [Fact]
    public async Task Index_ShouldMarkTweedsWrittenByCurrentUser()
    {
        var fixedZonedDateTime = new ZonedDateTime(new LocalDateTime(2022, 11, 18, 15, 20),
            DateTimeZone.Utc, new Offset());
        var tweed = new Data.Entities.Tweed
        {
            Likes = new List<Like>
                { new() { UserId = "user1", CreatedAt = fixedZonedDateTime } },
            AuthorId = "user2"
        };
        _tweedQueriesMock.Setup(t => t.GetLatestTweeds())
            .ReturnsAsync(new List<Data.Entities.Tweed> { tweed });
        var user = new AppUser
        {
            UserName = "User 2"
        };
        _userManagerMock.Setup(u => u.FindByIdAsync("user2")).ReturnsAsync(user);

        var result = await _homeController.Index();

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<IndexViewModel>(resultAsView.Model);
        var viewModel = (IndexViewModel)resultAsView.Model!;
        Assert.True(viewModel.Tweeds[0].LikedByCurrentUser);
    }
}

