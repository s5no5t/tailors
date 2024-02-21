using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tailors.Domain.ThreadAggregate;
using Tailors.Domain.TweedAggregate;
using Tailors.Domain.UserAggregate;
using Tailors.Domain.UserLikesAggregate;
using Tailors.Web.Features.Tweed;
using Tailors.Web.Helper;
using Tailors.Web.Test.Helper;
using Tailors.Web.Test.TestHelper;
using Xunit;

namespace Tailors.Web.Test.Controllers;

public class TweedControllerTest
{
    private static readonly DateTime FixedDateTime = new(2022, 11, 18, 15, 20, 0);
    private readonly CreateTweedUseCase _createTweedUseCase;
    private readonly ClaimsPrincipal _currentUserPrincipal = ControllerTestHelper.BuildPrincipal("currentUser");
    private readonly LikeTweedUseCase _likeTweedUseCase;
    private readonly NotificationManagerMock _notificationManagerMock;
    private readonly TweedController _sut;
    private readonly ThreadUseCase _threadUseCase;
    private readonly TweedRepositoryMock _tweedRepositoryMock = new();

    public TweedControllerTest()
    {
        _notificationManagerMock = new NotificationManagerMock();
        UserLikesRepositoryMock userLikesRepositoryMock = new();
        _likeTweedUseCase = new LikeTweedUseCase(userLikesRepositoryMock);
        var store = new UserStoreMock();
        store.Create(new AppUser { UserName = "currentUser" });
        store.Create(new AppUser { Id = "authorId", UserName = "author" });
        var userManager = UserManagerBuilder.CreateUserManager(store);
        _createTweedUseCase = new CreateTweedUseCase(_tweedRepositoryMock);
        _threadUseCase = new ThreadUseCase(_tweedRepositoryMock);
        TweedViewModelFactory tweedViewModelFactory = new(userLikesRepositoryMock, _likeTweedUseCase, userManager);

        _sut = new TweedController(_tweedRepositoryMock, userManager, tweedViewModelFactory)
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
    public async Task ShowThreadForTweed_ShouldReturnViewResult()
    {
        var rootTweed = new Tweed(id: "tweedId", text: string.Empty, createdAt: FixedDateTime, authorId: "authorId");
        await _tweedRepositoryMock.Create(rootTweed);

        var result = await _sut.ShowThreadForTweed("tweedId", _threadUseCase);

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<ShowThreadForTweedViewModel>(resultAsView.Model);
    }

    [Fact]
    public async Task ShowThreadForTweed_ShouldReturnTweed()
    {
        var rootTweed = new Tweed(id: "tweedId", text: string.Empty, createdAt: FixedDateTime, authorId: "authorId");
        await _tweedRepositoryMock.Create(rootTweed);

        var result = await _sut.ShowThreadForTweed("tweedId", _threadUseCase);

        var resultViewModel = (ShowThreadForTweedViewModel)((ViewResult)result).Model!;
        Assert.Equal(rootTweed.Id, resultViewModel.Tweeds[0].Id);
    }

    [Fact]
    public async Task ShowThreadForTweed_ShouldSetParentTweedId()
    {
        var rootTweed = new Tweed(id: "tweedId", text: string.Empty, createdAt: FixedDateTime, authorId: "authorId");
        await _tweedRepositoryMock.Create(rootTweed);

        var result = await _sut.ShowThreadForTweed("tweedId", _threadUseCase);

        var resultViewModel = (ShowThreadForTweedViewModel)((ViewResult)result).Model!;
        Assert.Equal("tweedId", resultViewModel.CreateReplyTweed.ParentTweedId);
    }

    [Fact]
    public async Task Create_ShouldReturnRedirect()
    {
        CreateTweedViewModel viewModel = new()
        {
            Text = "test"
        };
        var result = await _sut.Create(viewModel, _createTweedUseCase, _notificationManagerMock);

        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task Create_ShouldSaveTweed()
    {
        CreateTweedViewModel viewModel = new()
        {
            Text = "test"
        };

        await _sut.Create(viewModel, _createTweedUseCase, _notificationManagerMock);

        var tweeds = await _tweedRepositoryMock.GetAllByAuthorId("currentUser", 1);
        Assert.NotEmpty(tweeds);
    }

    [Fact]
    public async Task Create_ShouldSetSuccessMessage()
    {
        CreateTweedViewModel viewModel = new()
        {
            Text = "test"
        };
        await _sut.Create(viewModel, _createTweedUseCase, _notificationManagerMock);

        Assert.Equal("Tweed Posted", _notificationManagerMock.SuccessMessage);
    }

    [Fact]
    public async Task CreateReply_ShouldReturnRedirect()
    {
        await _tweedRepositoryMock.Create(new Tweed(text: string.Empty, createdAt: FixedDateTime, authorId: "authorId",
            id: "rootTweedId"));
        await _tweedRepositoryMock.Create(new Tweed(text: string.Empty, createdAt: FixedDateTime, authorId: "authorId",
            id: "parentTweedId"));

        CreateReplyTweedViewModel viewModel = new()
        {
            Text = "test",
            ParentTweedId = "parentTweedId"
        };

        var result =
            await _sut.CreateReply(viewModel, _createTweedUseCase, _notificationManagerMock);

        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task CreateReply_ShouldSaveReplyTweed()
    {
        await _tweedRepositoryMock.Create(new Tweed(text: string.Empty, createdAt: FixedDateTime, authorId: "authorId",
            id: "rootTweedId"));
        await _tweedRepositoryMock.Create(new Tweed(text: string.Empty, createdAt: FixedDateTime, authorId: "authorId",
            id: "parentTweedId"));
        CreateReplyTweedViewModel viewModel = new()
        {
            Text = "text",
            ParentTweedId = "parentTweedId"
        };

        await _sut.CreateReply(viewModel, _createTweedUseCase, _notificationManagerMock);

        var tweeds = await _tweedRepositoryMock.GetAllByAuthorId("currentUser", 1);
        Assert.NotEmpty(tweeds);
    }

    [Fact]
    public async Task CreateReply_ShouldSetSuccessMessage()
    {
        await _tweedRepositoryMock.Create(new Tweed(text: string.Empty, createdAt: FixedDateTime, authorId: "authorId",
            id: "rootTweedId"));
        await _tweedRepositoryMock.Create(new Tweed(text: string.Empty, createdAt: FixedDateTime, authorId: "authorId",
            id: "parentTweedId"));

        CreateReplyTweedViewModel viewModel = new()
        {
            Text = "test",
            ParentTweedId = "parentTweedId"
        };
        await _sut.CreateReply(viewModel, _createTweedUseCase, _notificationManagerMock);

        Assert.Equal("Reply Posted", _notificationManagerMock.SuccessMessage);
    }

    [Fact]
    public async Task CreateReply_ShouldReturnPartialView_WhenParentTweedIdIsMissing()
    {
        CreateReplyTweedViewModel viewModel = new()
        {
            Text = "test"
        };
        _sut.ValidateViewModel(viewModel);

        var result =
            await _sut.CreateReply(viewModel, _createTweedUseCase, _notificationManagerMock);

        Assert.IsType<PartialViewResult>(result);
    }

    [Fact]
    public async Task CreateReply_ShouldReturnBadRequest_WhenParentTweedDoesntExist()
    {
        CreateReplyTweedViewModel viewModel = new()
        {
            Text = "test",
            ParentTweedId = "nonExistingTweed"
        };

        var result =
            await _sut.CreateReply(viewModel, _createTweedUseCase, _notificationManagerMock);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Like_ShouldIncreaseLikes()
    {
        Tweed tweed = new("authorId", string.Empty, FixedDateTime, "123");
        await _tweedRepositoryMock.Create(tweed);

        await _sut.Like("123", false, _likeTweedUseCase);

        Assert.True(await _likeTweedUseCase.DoesUserLikeTweed(tweed.Id!, "currentUser"));
    }

    [Fact]
    public async Task Like_ShouldReturnPartialView()
    {
        Tweed tweed = new("authorId", string.Empty, FixedDateTime, "123");
        await _tweedRepositoryMock.Create(tweed);

        var result = await _sut.Like("123", false, _likeTweedUseCase);

        Assert.IsType<PartialViewResult>(result);
    }

    [Fact]
    public async Task Unlike_ShouldDecreaseLikes()
    {
        Tweed tweed = new("authorId", string.Empty, FixedDateTime, "123");
        await _tweedRepositoryMock.Create(tweed);

        await _sut.Unlike("123", false, _likeTweedUseCase);

        Assert.False(await _likeTweedUseCase.DoesUserLikeTweed(tweed.Id!, "currentUser"));
    }

    [Fact]
    public async Task Unlike_ShouldReturnPartialView()
    {
        Tweed tweed = new("authorId", string.Empty, FixedDateTime, "123");
        await _tweedRepositoryMock.Create(tweed);

        var result = await _sut.Unlike("123", false, _likeTweedUseCase);

        Assert.IsType<PartialViewResult>(result);
    }
}
