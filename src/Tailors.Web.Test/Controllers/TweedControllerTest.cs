using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tailors.Domain.ThreadAggregate;
using Tailors.Domain.TweedAggregate;
using Tailors.Domain.UserAggregate;
using Tailors.Domain.UserLikesAggregate;
using Tailors.Web.Features.Tweed;
using Tailors.Web.Helper;
using Tailors.Web.Test.TestHelper;
using Xunit;

namespace Tailors.Web.Test.Controllers;

public class TweedControllerTest
{
    private static readonly DateTime FixedDateTime = new(2022, 11, 18, 15, 20, 0);
    private readonly CreateTweedUseCase _createTweedUseCase;
    private readonly ClaimsPrincipal _currentUserPrincipal = ControllerTestHelper.BuildPrincipal();
    private readonly LikeTweedUseCase _likeTweedUseCase;
    private readonly Mock<INotificationManager> _notificationManagerMock = new();
    private readonly TweedController _sut;
    private readonly ThreadRepositoryMock _threadRepositoryMock = new();
    private readonly ThreadUseCase _threadUseCase;
    private readonly TweedRepositoryMock _tweedRepositoryMock = new();

    private readonly Mock<UserManager<AppUser>> _userManagerMock =
        UserManagerMockHelper.MockUserManager<AppUser>();

    public TweedControllerTest()
    {
        UserLikesRepositoryMock userLikesRepositoryMock = new();
        _likeTweedUseCase = new LikeTweedUseCase(userLikesRepositoryMock);
        _userManagerMock.Setup(u => u.GetUserId(_currentUserPrincipal)).Returns("currentUser");
        _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(new AppUser { UserName = "author" });
        _createTweedUseCase = new CreateTweedUseCase(_tweedRepositoryMock);
        _threadUseCase = new ThreadUseCase(_threadRepositoryMock, _tweedRepositoryMock);
        TweedViewModelFactory tweedViewModelFactory =
            new(userLikesRepositoryMock, _likeTweedUseCase, _userManagerMock.Object);

        _sut = new TweedController(_tweedRepositoryMock, _userManagerMock.Object, tweedViewModelFactory)
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
        var rootTweed = new Tweed(id: "tweedId", text: string.Empty, createdAt: FixedDateTime, authorId: "authorId",
            threadId: "threadId");
        await _tweedRepositoryMock.Create(rootTweed);
        var thread = new TailorsThread("threadId");
        thread.AddTweed(rootTweed);
        await _threadRepositoryMock.Create(thread);

        var result =
            await _sut.ShowThreadForTweed("tweedId", _threadUseCase);

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
        var result = await _sut.Create(viewModel, _createTweedUseCase, _notificationManagerMock.Object);

        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task Create_ShouldSaveTweed()
    {
        CreateTweedViewModel viewModel = new()
        {
            Text = "test"
        };

        await _sut.Create(viewModel, _createTweedUseCase, _notificationManagerMock.Object);

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
        await _sut.Create(viewModel, _createTweedUseCase, _notificationManagerMock.Object);

        _notificationManagerMock.Verify(n => n.AppendSuccess("Tweed Posted"));
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
            await _sut.CreateReply(viewModel, _createTweedUseCase, _notificationManagerMock.Object);

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

        await _sut.CreateReply(viewModel, _createTweedUseCase, _notificationManagerMock.Object);

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
        await _sut.CreateReply(viewModel, _createTweedUseCase, _notificationManagerMock.Object);

        _notificationManagerMock.Verify(n => n.AppendSuccess("Reply Posted"));
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
            await _sut.CreateReply(viewModel, _createTweedUseCase, _notificationManagerMock.Object);

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
            await _sut.CreateReply(viewModel, _createTweedUseCase, _notificationManagerMock.Object);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Like_ShouldIncreaseLikes()
    {
        Tweed tweed = new("author", string.Empty, FixedDateTime, "123");
        await _tweedRepositoryMock.Create(tweed);

        await _sut.Like("123", false, _likeTweedUseCase);

        Assert.True(await _likeTweedUseCase.DoesUserLikeTweed(tweed.Id!, "currentUser"));
    }

    [Fact]
    public async Task Like_ShouldReturnPartialView()
    {
        Tweed tweed = new("author", string.Empty, FixedDateTime, "123");
        await _tweedRepositoryMock.Create(tweed);
        _userManagerMock.Setup(u => u.FindByIdAsync("author")).ReturnsAsync(new AppUser());

        var result = await _sut.Like("123", false, _likeTweedUseCase);

        Assert.IsType<PartialViewResult>(result);
    }

    [Fact]
    public async Task Unlike_ShouldDecreaseLikes()
    {
        Tweed tweed = new("author", string.Empty, FixedDateTime, "123");
        await _tweedRepositoryMock.Create(tweed);

        await _sut.Unlike("123", false, _likeTweedUseCase);

        Assert.False(await _likeTweedUseCase.DoesUserLikeTweed(tweed.Id!, "currentUser"));
    }

    [Fact]
    public async Task Unlike_ShouldReturnPartialView()
    {
        Tweed tweed = new("authorId", string.Empty, FixedDateTime, "123");
        await _tweedRepositoryMock.Create(tweed);
        _userManagerMock.Setup(u => u.FindByIdAsync("authorId")).ReturnsAsync(new AppUser());

        var result = await _sut.Unlike("123", false, _likeTweedUseCase);

        Assert.IsType<PartialViewResult>(result);
    }
}
