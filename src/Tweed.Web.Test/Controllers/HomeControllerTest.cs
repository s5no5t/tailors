using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tweed.Data;
using Tweed.Data.Model;
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
    private readonly Mock<IFeedBuilder> _feedBuilderMock = new();
    private readonly HomeController _homeController;

    private readonly Mock<UserManager<AppUser>> _userManagerMock =
        UserManagerMockHelper.MockUserManager<AppUser>();

    private readonly Mock<IViewModelFactory> _viewModelFactoryMock = new();

    public HomeControllerTest()
    {
        _homeController = new HomeController(_feedBuilderMock.Object, _userManagerMock.Object,
            _viewModelFactoryMock.Object)
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
    public async Task Index_ShouldReturnIndexViewModel()
    {
        var tweed = new Data.Model.Tweed
        {
            Id = "tweedId",
            AuthorId = "author"
        };
        var appUser = new AppUser
        {
            Id = "currentUser"
        };
        _userManagerMock.Setup(u => u.GetUserAsync(_currentUserPrincipal)).ReturnsAsync(appUser);
        _feedBuilderMock.Setup(t => t.GetFeed("currentUser"))
            .ReturnsAsync(new List<Data.Model.Tweed> { tweed });
        _viewModelFactoryMock.Setup(v => v.BuildTweedViewModel(tweed))
            .ReturnsAsync(new TweedViewModel());

        var result = await _homeController.Index();

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<IndexViewModel>(resultAsView.Model);
    }
}
