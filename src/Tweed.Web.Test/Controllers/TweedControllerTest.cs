using System;
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
using Tweed.Web.Views.Search;
using Tweed.Web.Views.Tweed;
using Xunit;

namespace Tweed.Web.Test.Controllers;

public class TweedControllerTest
{
    private readonly ClaimsPrincipal _currentUserPrincipal = ControllerTestHelper.BuildPrincipal();
    private readonly Mock<IIdentityUserQueries> _appUserQueriesMock = new();
    private readonly Mock<INotificationManager> _notificationManagerMock = new();
    private readonly Mock<ITweedQueries> _tweedQueriesMock = new();
    private readonly Mock<UserManager<TweedIdentityUser>> _userManagerMock = UserManagerMockHelper.MockUserManager<TweedIdentityUser>();
    private readonly Mock<IViewModelFactory> _viewModelFactoryMock = new();
    private readonly TweedController _tweedController;

    public TweedControllerTest()
    {
        _userManagerMock.Setup(u => u.GetUserId(_currentUserPrincipal)).Returns("currentUser");
        _tweedQueriesMock.Setup(t => t.GetLikesCount(It.IsAny<string>())).ReturnsAsync(0);
        _tweedController = new TweedController(_tweedQueriesMock.Object, _userManagerMock.Object,
            _notificationManagerMock.Object, _appUserQueriesMock.Object, _viewModelFactoryMock.Object)
        {
            ControllerContext = ControllerTestHelper.BuildControllerContext(_currentUserPrincipal)
        };
    }

    [Fact]
    public void RequiresAuthorization()
    {
        var authorizeAttributeValue =
            Attribute.GetCustomAttribute(typeof(TweedController), typeof(AuthorizeAttribute));
        Assert.NotNull(authorizeAttributeValue);
    }


    [Fact]
    public async Task GetById_ShouldReturnGetByIdViewResult()
    {
        Data.Entities.Tweed tweed = new()
        {
            Id = "123"
        };
        _tweedQueriesMock.Setup(t => t.GetById(It.IsAny<string>())).ReturnsAsync(tweed);

        var result = await _tweedController.GetById("123");

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<GetByIdViewModel>(resultAsView.Model);
    }

    [Fact]
    public async Task Create_ShouldReturnRedirect()
    {
        CreateViewModel viewModel = new()
        {
            Text = "test"
        };
        var result = await _tweedController.Create(viewModel);

        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task Create_ShouldSaveTweed()
    {
        CreateViewModel viewModel = new()
        {
            Text = "text"
        };
        await _tweedController.Create(viewModel);

        _tweedQueriesMock.Verify(t =>
            t.StoreTweed("text", "currentUser", It.IsAny<ZonedDateTime>()));
    }

    [Fact]
    public async Task Create_ShouldSetSuccessMessage()
    {
        CreateViewModel viewModel = new()
        {
            Text = "test"
        };
        await _tweedController.Create(viewModel);

        _notificationManagerMock.Verify(n => n.AppendSuccess("Tweed Posted"));
    }

    [Fact]
    public async Task Like_ShouldIncreaseLikes()
    {
        Data.Entities.Tweed tweed = new()
        {
            AuthorId = "author"
        };
        _tweedQueriesMock.Setup(t => t.GetById("123")).ReturnsAsync(tweed);

        await _tweedController.Like("123");

        _appUserQueriesMock.Verify(u => u.AddLike("123", "currentUser", It.IsAny<ZonedDateTime>()));
    }

    [Fact]
    public async Task Like_ShouldReturnPartialView()
    {
        Data.Entities.Tweed tweed = new()
        {
            AuthorId = "author"
        };
        _tweedQueriesMock.Setup(t => t.GetById("123")).ReturnsAsync(tweed);
        _userManagerMock.Setup(u => u.FindByIdAsync("author")).ReturnsAsync(new TweedIdentityUser());

        var result = await _tweedController.Like("123");

        Assert.IsType<PartialViewResult>(result);
    }

    [Fact]
    public async Task Unlike_ShouldDecreaseLikes()
    {
        Data.Entities.Tweed tweed = new()
        {
            AuthorId = "author"
        };
        _tweedQueriesMock.Setup(t => t.GetById("123")).ReturnsAsync(tweed);

        await _tweedController.Unlike("123");

        _appUserQueriesMock.Verify(u => u.RemoveLike("123", "currentUser"));
    }

    [Fact]
    public async Task Unlike_ShouldReturnPartialView()
    {
        Data.Entities.Tweed tweed = new()
        {
            AuthorId = "author"
        };
        _tweedQueriesMock.Setup(t => t.GetById("123")).ReturnsAsync(tweed);
        _userManagerMock.Setup(u => u.FindByIdAsync("author")).ReturnsAsync(new TweedIdentityUser());

        var result = await _tweedController.Unlike("123");

        Assert.IsType<PartialViewResult>(result);
    }
}
