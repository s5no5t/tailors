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
using Tweed.Web.Helper;
using Tweed.Web.Test.TestHelper;
using Tweed.Web.Views.Home;
using Tweed.Web.Views.Shared;
using Xunit;

namespace Tweed.Web.Test.Controllers;

public class HomeControllerTest
{
    private readonly ClaimsPrincipal _currentUserPrincipal = ControllerTestHelper.BuildPrincipal();
    private readonly HomeController _homeController;
    private readonly Mock<ITweedQueries> _tweedQueriesMock = new();
    private readonly Mock<UserManager<TweedIdentityUser>> _userManagerMock = UserManagerMockHelper.MockUserManager<TweedIdentityUser>();
    private readonly Mock<IViewModelFactory> _viewModelFactoryMock = new();
    
    public HomeControllerTest()
    {
        _homeController = new HomeController(_tweedQueriesMock.Object, _userManagerMock.Object, _viewModelFactoryMock.Object)
        {
            ControllerContext = ControllerTestHelper.BuildControllerContext(_currentUserPrincipal)
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
    public async Task Index_ShouldSetLikedByCurrentUser()
    {
        var fixedZonedDateTime = new ZonedDateTime(new LocalDateTime(2022, 11, 18, 15, 20),
            DateTimeZone.Utc, new Offset());
        var tweed = new Data.Entities.Tweed
        {
            Id = "tweedId",
            AuthorId = "author"
        };
        var appUser = new TweedIdentityUser
        {
            Id = "currentUser",
            Likes = new List<TweedLike>
            {
                new()
                {
                    TweedId = "tweedId"
                }
            }
        };
        _userManagerMock.Setup(u => u.GetUserAsync(_currentUserPrincipal)).ReturnsAsync(appUser);
        _tweedQueriesMock.Setup(t => t.GetFeed("currentUser"))
            .ReturnsAsync(new List<Data.Entities.Tweed> { tweed });
        _viewModelFactoryMock.Setup(v => v.BuildTweedViewModel(tweed)).ReturnsAsync(new TweedViewModel());

        var result = await _homeController.Index();

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<IndexViewModel>(resultAsView.Model);
    }
}

