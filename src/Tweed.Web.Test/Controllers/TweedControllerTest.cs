using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NodaTime;
using Tweed.Data.Domain;
using Tweed.Data.Model;
using Tweed.Web.Controllers;
using Tweed.Web.Helper;
using Tweed.Web.Test.TestHelper;
using Tweed.Web.Views.Tweed;
using Xunit;

namespace Tweed.Web.Test.Controllers;

public class TweedControllerTest
{
    private readonly Mock<IAppUserLikesService> _appUserLikesQueriesMock = new();
    private readonly Mock<IAppUserService> _appUserQueriesMock = new();
    private readonly ClaimsPrincipal _currentUserPrincipal = ControllerTestHelper.BuildPrincipal();
    private readonly Mock<INotificationManager> _notificationManagerMock = new();
    private readonly TweedController _tweedController;
    private readonly Mock<ITweedService> _tweedQueriesMock = new();
    private readonly Mock<ITweedThreadService> _tweedTheadServiceMock = new();

    private readonly Mock<UserManager<AppUser>> _userManagerMock =
        UserManagerMockHelper.MockUserManager<AppUser>();

    private readonly Mock<IViewModelFactory> _viewModelFactoryMock = new();

    public TweedControllerTest()
    {
        _userManagerMock.Setup(u => u.GetUserId(_currentUserPrincipal)).Returns("currentUser");
        _tweedQueriesMock.Setup(t => t.GetLikesCount(It.IsAny<string>())).ReturnsAsync(0);
        _tweedQueriesMock.Setup(t => t.StoreTweed(It.IsAny<Data.Model.Tweed>()));
        _tweedController = new TweedController(_tweedQueriesMock.Object, _userManagerMock.Object,
            _notificationManagerMock.Object, _appUserQueriesMock.Object,
            _appUserLikesQueriesMock.Object, _viewModelFactoryMock.Object,
            _tweedTheadServiceMock.Object)
        {
            ControllerContext = ControllerTestHelper.BuildControllerContext(_currentUserPrincipal),
            Url = new Mock<IUrlHelper>().Object
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
        Data.Model.Tweed tweed = new()
        {
            Id = "tweedId"
        };
        _tweedQueriesMock.Setup(t => t.GetById(It.IsAny<string>())).ReturnsAsync(tweed);

        var result = await _tweedController.GetById("tweedId");

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<GetByIdViewModel>(resultAsView.Model);
    }

    [Fact]
    public async Task GetById_ShouldSetParentTweedId()
    {
        Data.Model.Tweed tweed = new()
        {
            Id = "tweeds/1"
        };
        _tweedQueriesMock.Setup(t => t.GetById(It.IsAny<string>())).ReturnsAsync(tweed);

        var result = await _tweedController.GetById(HttpUtility.UrlEncode(tweed.Id));

        var resultViewModel = (GetByIdViewModel)((ViewResult)result).Model!;
        Assert.Equal(tweed.Id, resultViewModel.CreateTweed.ParentTweedId);
    }

    [Fact]
    public async Task Create_ShouldReturnRedirect()
    {
        CreateTweedViewModel viewModel = new()
        {
            Text = "test"
        };
        var result = await _tweedController.Create(viewModel);

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Create_ShouldSaveTweed()
    {
        CreateTweedViewModel viewModel = new()
        {
            Text = "test"
        };
        await _tweedController.Create(viewModel);

        _tweedQueriesMock.Verify(t => t.StoreTweed(It.IsAny<Data.Model.Tweed>()));
    }

    [Fact]
    public async Task Create_ShouldSaveThread()
    {
        CreateTweedViewModel viewModel = new()
        {
            Text = "test"
        };
        await _tweedController.Create(viewModel);

        _tweedTheadServiceMock.Verify(t => t.StoreThread(It.IsAny<TweedThread>()));
    }

    [Fact]
    public async Task Create_ShouldSetSuccessMessage()
    {
        CreateTweedViewModel viewModel = new()
        {
            Text = "test"
        };
        await _tweedController.Create(viewModel);

        _notificationManagerMock.Verify(n => n.AppendSuccess("Tweed Posted"));
    }

    [Fact]
    public async Task CreateReply_ShouldReturnRedirect()
    {
        _tweedQueriesMock.Setup(t => t.GetById("parentTweedId"))
            .ReturnsAsync(new Data.Model.Tweed());
        _tweedQueriesMock.Setup(t => t.GetById("rootTweedId"))
            .ReturnsAsync(new Data.Model.Tweed());

        CreateTweedViewModel viewModel = new()
        {
            Text = "test",
            ParentTweedId = "parentTweedId"
        };
        var result = await _tweedController.CreateReply(viewModel);

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task CreateReply_ShouldSaveTweed()
    {
        _tweedQueriesMock.Setup(t => t.GetById("parentTweedId"))
            .ReturnsAsync(new Data.Model.Tweed());
        _tweedQueriesMock.Setup(t => t.GetById("rootTweedId"))
            .ReturnsAsync(new Data.Model.Tweed());

        CreateTweedViewModel viewModel = new()
        {
            Text = "text",
            ParentTweedId = "parentTweedId"
        };
        await _tweedController.CreateReply(viewModel);

        _tweedQueriesMock.Verify(t => t.StoreTweed(It.IsAny<Data.Model.Tweed>()));
    }

    [Fact]
    public async Task CreateReply_ShouldSetSuccessMessage()
    {
        _tweedQueriesMock.Setup(t => t.GetById("parentTweedId"))
            .ReturnsAsync(new Data.Model.Tweed());
        _tweedQueriesMock.Setup(t => t.GetById("rootTweedId"))
            .ReturnsAsync(new Data.Model.Tweed());

        CreateTweedViewModel viewModel = new()
        {
            Text = "test",
            ParentTweedId = "parentTweedId"
        };
        await _tweedController.CreateReply(viewModel);

        _notificationManagerMock.Verify(n => n.AppendSuccess("Reply Posted"));
    }

    [Fact]
    public async Task CreateReply_ShouldReturnBadRequest_WhenParentTweedIdIsMissing()
    {
        CreateTweedViewModel viewModel = new()
        {
            Text = "test"
        };
        var result = await _tweedController.CreateReply(viewModel);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task CreateReply_ShouldReturnBadRequest_WhenParentTweedDoesntExist()
    {
        CreateTweedViewModel viewModel = new()
        {
            Text = "test",
            ParentTweedId = "nonExistingTweed"
        };
        var result = await _tweedController.CreateReply(viewModel);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Like_ShouldIncreaseLikes()
    {
        Data.Model.Tweed tweed = new()
        {
            AuthorId = "author"
        };
        _tweedQueriesMock.Setup(t => t.GetById("123")).ReturnsAsync(tweed);

        await _tweedController.Like("123");

        _appUserLikesQueriesMock.Verify(u =>
            u.AddLike("123", "currentUser", It.IsAny<ZonedDateTime>()));
    }

    [Fact]
    public async Task Like_ShouldReturnPartialView()
    {
        Data.Model.Tweed tweed = new()
        {
            AuthorId = "author"
        };
        _tweedQueriesMock.Setup(t => t.GetById("123")).ReturnsAsync(tweed);
        _userManagerMock.Setup(u => u.FindByIdAsync("author")).ReturnsAsync(new AppUser());

        var result = await _tweedController.Like("123");

        Assert.IsType<PartialViewResult>(result);
    }

    [Fact]
    public async Task Unlike_ShouldDecreaseLikes()
    {
        Data.Model.Tweed tweed = new()
        {
            AuthorId = "author"
        };
        _tweedQueriesMock.Setup(t => t.GetById("123")).ReturnsAsync(tweed);

        await _tweedController.Unlike("123");

        _appUserLikesQueriesMock.Verify(u => u.RemoveLike("123", "currentUser"));
    }

    [Fact]
    public async Task Unlike_ShouldReturnPartialView()
    {
        Data.Model.Tweed tweed = new()
        {
            AuthorId = "author"
        };
        _tweedQueriesMock.Setup(t => t.GetById("123")).ReturnsAsync(tweed);
        _userManagerMock.Setup(u => u.FindByIdAsync("author")).ReturnsAsync(new AppUser());

        var result = await _tweedController.Unlike("123");

        Assert.IsType<PartialViewResult>(result);
    }
}
