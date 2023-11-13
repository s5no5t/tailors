using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tailors.Domain.ThreadAggregate;
using Tailors.Domain.TweedAggregate;
using Tailors.Domain.UserAggregate;
using Tailors.Domain.UserFollowsAggregate;
using Tailors.Domain.UserLikesAggregate;
using Tailors.Web.Features.Feed;
using Tailors.Web.Helper;
using Tailors.Web.Test.TestHelper;
using Xunit;

namespace Tailors.Web.Test.Controllers;

public class FeedControllerTest
{
    private readonly FeedController _sut;

    private readonly TweedRepositoryMock _tweedRepositoryMock = new();

    public FeedControllerTest()
    {
        var store = new UserStoreMock();
        var userManagerMock = UserManagerBuilder.CreateUserManager(store);
        var user = new AppUser
        {
            Id = "currentUser"
        };
        store.Create(user);
        var userFollowsRepository = new UserFollowsRepositoryMock();
        FollowUserUseCase followUserUseCase = new(userFollowsRepository);
        var showFeedUseCase = new ShowFeedUseCase(_tweedRepositoryMock, followUserUseCase);
        UserLikesRepositoryMock userLikesRepositoryMock = new();
        var viewModelFactory = new TweedViewModelFactory(userLikesRepositoryMock,
            new LikeTweedUseCase(userLikesRepositoryMock), userManagerMock);
        var currentUserPrincipal = ControllerTestHelper.BuildPrincipal(user.Id);

        _sut = new FeedController(showFeedUseCase, userManagerMock, viewModelFactory)
        {
            ControllerContext = ControllerTestHelper.BuildControllerContext(currentUserPrincipal)
        };
    }

    [Fact]
    public void RequiresAuthorization()
    {
        var authorizeAttributeValue =
            Attribute.GetCustomAttribute(typeof(FeedController), typeof(AuthorizeAttribute));
        Assert.NotNull(authorizeAttributeValue);
    }

    [Fact]
    public async Task Index_ShouldReturnIndexViewModel()
    {
        var result = await _sut.Index();

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<IndexViewModel>(resultAsView.Model);
    }

    [Fact]
    public async Task Index_ShouldReturnPage0()
    {
        var result = await _sut.Index();

        var model = ((result as ViewResult)!.Model as IndexViewModel)!;
        Assert.Equal(0, model.Feed.Page);
    }

    [Fact]
    public async Task Index_ShouldReturnTweeds()
    {
        var tweed = new Tweed("currentUser", id: "tweedId", createdAt: DateTime.Now, text: string.Empty);
        await _tweedRepositoryMock.Create(tweed);

        var result = await _sut.Index();

        var model = ((result as ViewResult)!.Model as IndexViewModel)!;
        Assert.Equal(tweed.Id, model.Feed.Tweeds[0].Id);
    }

    [Fact]
    public async Task Feed_ShouldReturnFeedPartialViewModel()
    {
        var result = await _sut.Feed();

        Assert.IsType<PartialViewResult>(result);
        var resultAsView = (PartialViewResult)result;
        Assert.IsType<FeedViewModel>(resultAsView.Model);
    }

    [Fact]
    public async Task Feed_ShouldReturnPage1_WhenPageIs1()
    {
        var result = await _sut.Feed(1);

        var model = ((result as PartialViewResult)!.Model as FeedViewModel)!;

        Assert.Equal(1, model.Page);
    }

    [Fact]
    public async Task Feed_ShouldReturnTweeds()
    {
        var tweed = new Tweed("currentUser", id: "tweedId", createdAt: DateTime.Now, text: string.Empty);
        await _tweedRepositoryMock.Create(tweed);

        var result = await _sut.Feed();

        var model = ((result as PartialViewResult)!.Model as FeedViewModel)!;
        Assert.Equal(tweed.Id, model.Tweeds[0].Id);
    }

    [Fact]
    public async Task UpdateAvailable_ShouldReturnTrue_WhenThereIsANewTweed()
    {
        var instant = new DateTime(2023, 5, 22, 10, 0, 0);
        var tweed = new Tweed(id: "tweedId", createdAt: instant.AddMinutes(5), authorId: "authorId",
            text: string.Empty);
        await _tweedRepositoryMock.Create(tweed);
        _sut.ControllerContext.HttpContext.Request.Headers["Hx-Request"] = "true";

        var result = await _sut.NewTweedsNotification(instant);

        var resultViewModel = (NewTweedsNotificationViewModel)((PartialViewResult)result).Model!;
        Assert.True(resultViewModel.NewTweedsAvailable);
    }

    [Fact]
    public async Task UpdateAvailable_ShouldReturnFalse_WhenThereIsNoNewTweed()
    {
        var instant = new DateTime(2023, 5, 22, 10, 0, 0);
        var tweed = new Tweed(id: "tweedId", createdAt: instant.AddMinutes(-5), authorId: "authorId",
            text: string.Empty);
        await _tweedRepositoryMock.Create(tweed);
        _sut.ControllerContext.HttpContext.Request.Headers["Hx-Request"] = "true";

        var result = await _sut.NewTweedsNotification(instant);

        var resultViewModel = (NewTweedsNotificationViewModel)((PartialViewResult)result).Model!;
        Assert.False(resultViewModel.NewTweedsAvailable);
    }

    [Fact]
    public async Task UpdateAvailable_ShouldReturnRedirect_WhenRequestIsNotFromHtmx()
    {
        var instant = new DateTime(2023, 5, 22, 10, 0, 0);

        await Assert.ThrowsAsync<Exception>(async () =>
            await _sut.NewTweedsNotification(instant));
    }
}
