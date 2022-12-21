using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tweed.Data;
using Tweed.Data.Entities;
using Tweed.Web.Controllers;
using Tweed.Web.Views.Search;
using Xunit;

namespace Tweed.Web.Test.Controllers;

public class SearchControllerTest
{
    private readonly Mock<IAppUserQueries> _appUserQueriesMock;
    private readonly SearchController _searchController;
    private readonly Mock<ITweedQueries> _tweedQueriesMock;

    public SearchControllerTest()
    {
        _appUserQueriesMock = new Mock<IAppUserQueries>();
        _appUserQueriesMock.Setup(u => u.Search(It.IsAny<string>())).ReturnsAsync(new List<AppUser>());
        _tweedQueriesMock = new Mock<ITweedQueries>();
        _tweedQueriesMock.Setup(u => u.Search(It.IsAny<string>())).ReturnsAsync(new List<Data.Entities.Tweed>());
        _searchController = new SearchController(_appUserQueriesMock.Object, _tweedQueriesMock.Object);
    }

    [Fact]
    public void RequiresAuthorization()
    {
        var authorizeAttributeValue =
            Attribute.GetCustomAttribute(typeof(SearchController), typeof(AuthorizeAttribute));
        Assert.NotNull(authorizeAttributeValue);
    }

    [Fact]
    public async Task Index_ShouldReturnView()
    {
        var result = await _searchController.Index();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Results_ShouldSearchAppUsers()
    {
        AppUser user = new()
        {
            Id = "userId"
        };
        _appUserQueriesMock.Setup(u => u.Search("userId")).ReturnsAsync(new List<AppUser> { user });

        var result = await _searchController.Results("userId");

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<IndexViewModel>(resultAsView.Model);
        var viewModel = (IndexViewModel)resultAsView.Model!;
        Assert.Contains(viewModel.FoundUsers, u => u.UserId == user.Id);
    }

    [Fact]
    public async Task Results_ShouldReturnUserName()
    {
        AppUser user = new()
        {
            Id = "userId",
            UserName = "UserName"
        };
        _appUserQueriesMock.Setup(u => u.Search("userId")).ReturnsAsync(new List<AppUser> { user });

        var result = await _searchController.Results("userId");

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<IndexViewModel>(resultAsView.Model);
        var viewModel = (IndexViewModel)resultAsView.Model!;
        Assert.Equal("UserName", viewModel.FoundUsers[0].UserName);
    }

    [Fact]
    public async Task Results_ShouldSearchTweeds()
    {
        _tweedQueriesMock.Setup(u => u.Search("term")).ReturnsAsync(new List<Data.Entities.Tweed>
        {
            new()
            {
                Id = "tweedId"
            }
        });

        var result = await _searchController.Results("term");

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<IndexViewModel>(resultAsView.Model);
        var viewModel = (IndexViewModel)resultAsView.Model!;
        Assert.Contains(viewModel.FoundTweeds, t => t.TweedId == "tweedId");
    }
}
