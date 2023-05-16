using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NodaTime;
using Tweed.Domain;
using Tweed.Domain.Model;
using Tweed.Web.Controllers;
using Tweed.Web.Helper;
using Tweed.Web.Test.TestHelper;
using Tweed.Web.Views.Shared;
using Tweed.Web.Views.Tweed;
using Xunit;

namespace Tweed.Web.Test.Controllers;

public class TweedControllerTest
{
    private readonly ClaimsPrincipal _currentUserPrincipal = ControllerTestHelper.BuildPrincipal();
    private readonly Mock<INotificationManager> _notificationManagerMock = new();
    private readonly TweedController _tweedController;
    private readonly Mock<ILikeTweedUseCase> _likeTweedUseCaseMock = new();
    private readonly Mock<ITweedRepository> _tweedRepositoryMock = new();
    private readonly Mock<ICreateTweedUseCase> _createTweedUseCaseMock = new();
    private readonly Mock<IShowThreadUseCase> _showThreadUseCaseMock = new();

    private readonly Mock<UserManager<User>> _userManagerMock =
        UserManagerMockHelper.MockUserManager<User>();

    private readonly Mock<ITweedViewModelFactory> _tweedViewModelFactoryMock = new();

    public TweedControllerTest()
    {
        _userManagerMock.Setup(u => u.GetUserId(_currentUserPrincipal)).Returns("currentUser");
        _createTweedUseCaseMock.Setup(t =>
                t.CreateRootTweed(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<ZonedDateTime>()))
            .ReturnsAsync(new Domain.Model.Tweed
            {
                Id = "tweedId"
            });
        _createTweedUseCaseMock.Setup(t => t.CreateReplyTweed(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<ZonedDateTime>(), It.IsAny<string>())).ReturnsAsync(new Domain.Model.Tweed
        {
            Id = "tweedId"
        });
        _showThreadUseCaseMock
            .Setup(t => t.GetLeadingTweeds(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<TweedThread.TweedReference>());
        _tweedController = new TweedController(_tweedRepositoryMock.Object,
            _userManagerMock.Object,
            _notificationManagerMock.Object,
            _likeTweedUseCaseMock.Object, _showThreadUseCaseMock.Object,
            _tweedViewModelFactoryMock.Object)
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
    public async Task ShowThreadForTweed_ShouldReturnGetByIdViewResult()
    {
        Domain.Model.Tweed tweed = new()
        {
            Id = "tweedId"
        };
        _tweedRepositoryMock.Setup(t => t.GetById(It.IsAny<string>())).ReturnsAsync(tweed);

        var result = await _tweedController.ShowThreadForTweed("tweedId");

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<ShowThreadForTweedViewModel>(resultAsView.Model);
    }

    [Fact]
    public async Task ShowThreadForTweed_ShouldReturnCurrentTweed()
    {
        Domain.Model.Tweed tweed = new()
        {
            Id = "tweedId"
        };
        _tweedRepositoryMock.Setup(t => t.GetById(It.IsAny<string>())).ReturnsAsync(tweed);
        _tweedViewModelFactoryMock.Setup(v => v.Create(tweed)).ReturnsAsync(
            new TweedViewModel
            {
                Id = tweed.Id
            });

        var result = await _tweedController.ShowThreadForTweed("tweedId");

        var resultViewModel = (ShowThreadForTweedViewModel)((ViewResult)result).Model!;
        Assert.Equal(tweed.Id, resultViewModel.Tweed.Id);
    }

    [Fact]
    public async Task ShowThreadForTweed_ShouldReturnLeadingTweeds()
    {
        Domain.Model.Tweed rootTweed = new()
        {
            Id = "rootTweedId"
        };
        Domain.Model.Tweed tweed = new()
        {
            Id = "tweedId"
        };
        _showThreadUseCaseMock
            .Setup(t => t.GetLeadingTweeds(It.IsAny<string>(), tweed.Id)).ReturnsAsync(
                new List<TweedThread.TweedReference>
                {
                    new()
                    {
                        TweedId = rootTweed.Id
                    }
                });
        _tweedRepositoryMock.Setup(t => t.GetById(rootTweed.Id)).ReturnsAsync(rootTweed);
        _tweedRepositoryMock.Setup(t => t.GetById(tweed.Id)).ReturnsAsync(tweed);
        _tweedViewModelFactoryMock.Setup(v => v.Create(rootTweed)).ReturnsAsync(
            new TweedViewModel
            {
                Id = rootTweed.Id
            });
        _tweedViewModelFactoryMock.Setup(v => v.Create(tweed)).ReturnsAsync(
            new TweedViewModel
            {
                Id = tweed.Id
            });

        var result = await _tweedController.ShowThreadForTweed("tweedId");

        var resultViewModel = (ShowThreadForTweedViewModel)((ViewResult)result).Model!;
        Assert.Equal(resultViewModel.LeadingTweeds[0].Id, rootTweed.Id);
    }

    [Fact]
    public async Task ShowThreadForTweed_ShouldReturnReplies()
    {
        Domain.Model.Tweed tweed = new()
        {
            Id = "tweedId"
        };
        Domain.Model.Tweed replyTweed = new()
        {
            Id = "replyTweedId"
        };
        _tweedRepositoryMock.Setup(t => t.GetById(tweed.Id)).ReturnsAsync(tweed);
        _tweedViewModelFactoryMock.Setup(v => v.Create(tweed)).ReturnsAsync(
            new TweedViewModel
            {
                Id = tweed.Id
            });

        var result = await _tweedController.ShowThreadForTweed("tweedId");

        var resultViewModel = (ShowThreadForTweedViewModel)((ViewResult)result).Model!;
        Assert.Equal(resultViewModel.ReplyTweeds[0].Id, replyTweed.Id);
    }

    [Fact]
    public async Task ShowThreadForTweed_ShouldSetParentTweedId()
    {
        Domain.Model.Tweed tweed = new()
        {
            Id = "tweeds/1"
        };
        _tweedRepositoryMock.Setup(t => t.GetById(It.IsAny<string>())).ReturnsAsync(tweed);

        var result = await _tweedController.ShowThreadForTweed(HttpUtility.UrlEncode(tweed.Id));

        var resultViewModel = (ShowThreadForTweedViewModel)((ViewResult)result).Model!;
        Assert.Equal(tweed.Id, resultViewModel.CreateTweed.ParentTweedId);
    }

    [Fact]
    public async Task Create_ShouldReturnRedirect()
    {
        CreateTweedViewModel viewModel = new()
        {
            Text = "test"
        };
        var result = await _tweedController.Create(viewModel, _createTweedUseCaseMock.Object);

        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task Create_ShouldSaveTweed()
    {
        CreateTweedViewModel viewModel = new()
        {
            Text = "test"
        };
        await _tweedController.Create(viewModel, _createTweedUseCaseMock.Object);

        _createTweedUseCaseMock.Verify(t =>
            t.CreateRootTweed(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZonedDateTime>()));
    }


    [Fact]
    public async Task Create_ShouldSetSuccessMessage()
    {
        CreateTweedViewModel viewModel = new()
        {
            Text = "test"
        };
        await _tweedController.Create(viewModel, _createTweedUseCaseMock.Object);

        _notificationManagerMock.Verify(n => n.AppendSuccess("Tweed Posted"));
    }

    [Fact]
    public async Task CreateReply_ShouldReturnRedirect()
    {
        _tweedRepositoryMock.Setup(t => t.GetById("parentTweedId"))
            .ReturnsAsync(new Domain.Model.Tweed());
        _tweedRepositoryMock.Setup(t => t.GetById("rootTweedId"))
            .ReturnsAsync(new Domain.Model.Tweed());

        CreateReplyTweedViewModel viewModel = new()
        {
            Text = "test",
            ParentTweedId = "parentTweedId"
        };
        var result = await _tweedController.CreateReply(viewModel, _createTweedUseCaseMock.Object);

        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task CreateReply_ShouldSaveReplyTweed()
    {
        _tweedRepositoryMock.Setup(t => t.GetById("parentTweedId"))
            .ReturnsAsync(new Domain.Model.Tweed());
        _tweedRepositoryMock.Setup(t => t.GetById("rootTweedId"))
            .ReturnsAsync(new Domain.Model.Tweed());

        CreateReplyTweedViewModel viewModel = new()
        {
            Text = "text",
            ParentTweedId = "parentTweedId"
        };
        await _tweedController.CreateReply(viewModel, _createTweedUseCaseMock.Object);

        _createTweedUseCaseMock.Verify(t => t.CreateReplyTweed(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<ZonedDateTime>(), It.IsAny<string>()));
    }

    [Fact]
    public async Task CreateReply_ShouldSetSuccessMessage()
    {
        _tweedRepositoryMock.Setup(t => t.GetById("parentTweedId"))
            .ReturnsAsync(new Domain.Model.Tweed());
        _tweedRepositoryMock.Setup(t => t.GetById("rootTweedId"))
            .ReturnsAsync(new Domain.Model.Tweed());

        CreateReplyTweedViewModel viewModel = new()
        {
            Text = "test",
            ParentTweedId = "parentTweedId"
        };
        await _tweedController.CreateReply(viewModel, _createTweedUseCaseMock.Object);

        _notificationManagerMock.Verify(n => n.AppendSuccess("Reply Posted"));
    }

    [Fact]
    public async Task CreateReply_ShouldReturnBadRequest_WhenParentTweedIdIsMissing()
    {
        CreateReplyTweedViewModel viewModel = new()
        {
            Text = "test"
        };
        var result = await _tweedController.CreateReply(viewModel, _createTweedUseCaseMock.Object);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task CreateReply_ShouldReturnBadRequest_WhenParentTweedDoesntExist()
    {
        CreateReplyTweedViewModel viewModel = new()
        {
            Text = "test",
            ParentTweedId = "nonExistingTweed"
        };
        var result = await _tweedController.CreateReply(viewModel, _createTweedUseCaseMock.Object);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Like_ShouldIncreaseLikes()
    {
        Domain.Model.Tweed tweed = new()
        {
            AuthorId = "author"
        };
        _tweedRepositoryMock.Setup(t => t.GetById("123")).ReturnsAsync(tweed);

        await _tweedController.Like("123");

        _likeTweedUseCaseMock.Verify(u =>
            u.AddLike("123", "currentUser", It.IsAny<ZonedDateTime>()));
    }

    [Fact]
    public async Task Like_ShouldReturnPartialView()
    {
        Domain.Model.Tweed tweed = new()
        {
            AuthorId = "author"
        };
        _tweedRepositoryMock.Setup(t => t.GetById("123")).ReturnsAsync(tweed);
        _userManagerMock.Setup(u => u.FindByIdAsync("author")).ReturnsAsync(new User());

        var result = await _tweedController.Like("123");

        Assert.IsType<PartialViewResult>(result);
    }

    [Fact]
    public async Task Unlike_ShouldDecreaseLikes()
    {
        Domain.Model.Tweed tweed = new()
        {
            AuthorId = "author"
        };
        _tweedRepositoryMock.Setup(t => t.GetById("123")).ReturnsAsync(tweed);

        await _tweedController.Unlike("123");

        _likeTweedUseCaseMock.Verify(u => u.RemoveLike("123", "currentUser"));
    }

    [Fact]
    public async Task Unlike_ShouldReturnPartialView()
    {
        Domain.Model.Tweed tweed = new()
        {
            AuthorId = "author"
        };
        _tweedRepositoryMock.Setup(t => t.GetById("123")).ReturnsAsync(tweed);
        _userManagerMock.Setup(u => u.FindByIdAsync("author")).ReturnsAsync(new User());

        var result = await _tweedController.Unlike("123");

        Assert.IsType<PartialViewResult>(result);
    }
}
