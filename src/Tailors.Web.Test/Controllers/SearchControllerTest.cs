using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tailors.Domain.TweedAggregate;
using Tailors.Domain.UserAggregate;
using Tailors.Domain.UserLikesAggregate;
using Tailors.Web.Features.Search;
using Tailors.Web.Features.Tweed;
using Tailors.Web.Helper;
using Tailors.Web.Test.TestHelper;
using Xunit;

namespace Tailors.Web.Test.Controllers;

public class SearchControllerTest
{
    private readonly SearchController _sut;
    private readonly TweedRepositoryMock _tweedRepositoryMock = new();
    private readonly UserRepositoryMock _userRepositoryMock = new();

    public SearchControllerTest()
    {
        var userRepositoryMock = new UserRepositoryMock();
        var user = new AppUser("UserName", "user@example.com", "currentUser");
        userRepositoryMock.Create(user);
        UserLikesRepositoryMock userLikesRepositoryMock = new();
        var viewModelFactory = new TweedViewModelFactory(userLikesRepositoryMock,
            new LikeTweedUseCase(userLikesRepositoryMock), userRepositoryMock);

        var currentUserPrincipal = ControllerTestHelper.BuildPrincipal(user.Id!);

        _sut = new SearchController(_tweedRepositoryMock, _userRepositoryMock, viewModelFactory)
        {
            ControllerContext = ControllerTestHelper.BuildControllerContext(currentUserPrincipal)
        };
    }

    [Fact]
    public void RequiresAuthorization()
    {
        var authorizeAttributeValue =
            Attribute.GetCustomAttribute(typeof(SearchController), typeof(AuthorizeAttribute));
        Assert.NotNull(authorizeAttributeValue);
    }

    [Fact]
    public async Task Results_ShouldSearchUsers()
    {
        AppUser user = new("UserName", "user@example.com", "userId");
        await _userRepositoryMock.Create(user);

        var result = await _sut.Results("UserName", SearchKind.Users);

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<ResultsViewModel>(resultAsView.Model);
        var viewModel = (ResultsViewModel)resultAsView.Model!;
        Assert.Contains(viewModel.FoundUsers, u => u.UserId == user.Id);
    }

    [Fact]
    public async Task Results_ShouldReturnUserName()
    {
        AppUser user = new("UserName", "user@example.com", "userId");
        await _userRepositoryMock.Create(user);

        var result = await _sut.Results("UserName", SearchKind.Users);

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<ResultsViewModel>(resultAsView.Model);
        var viewModel = (ResultsViewModel)resultAsView.Model!;
        Assert.Equal("UserName", viewModel.FoundUsers[0].UserName);
    }

    [Fact]
    public async Task Results_ShouldSearchTweeds()
    {
        await _tweedRepositoryMock.Create(new Tweed(id: "tweedId", authorId: "authorId", text: "term",
            createdAt: DateTime.Now));

        var result = await _sut.Results("term", SearchKind.Tweeds);

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<ResultsViewModel>(resultAsView.Model);
        var viewModel = (ResultsViewModel)resultAsView.Model!;
        Assert.Contains(viewModel.FoundTweeds, t => t.Id == "tweedId");
    }
}
