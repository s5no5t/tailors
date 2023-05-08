using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tweed.Data.Domain;
using Tweed.Data.Model;
using Tweed.Web.Controllers;
using Tweed.Web.Views.Search;
using Xunit;

namespace Tweed.Web.Test.Controllers;

public class SearchControllerTest
{
    private readonly Mock<ISearchService> _appUserQueriesMock;
    private readonly SearchController _searchController;
    private readonly Mock<ITweedService> _tweedQueriesMock;

    public SearchControllerTest()
    {
        _appUserQueriesMock = new Mock<ISearchService>();
        _appUserQueriesMock.Setup(u => u.SearchAppUsers(It.IsAny<string>()))
            .ReturnsAsync(new List<AppUser>());
        _appUserQueriesMock.Setup(u => u.SearchTweeds(It.IsAny<string>()))
            .ReturnsAsync(new List<Data.Model.Tweed>());
        _tweedQueriesMock = new Mock<ITweedService>();
        _searchController =
            new SearchController(_appUserQueriesMock.Object, _tweedQueriesMock.Object);
    }

    [Fact]
    public void RequiresAuthorization()
    {
        var authorizeAttributeValue =
            Attribute.GetCustomAttribute(typeof(SearchController), typeof(AuthorizeAttribute));
        Assert.NotNull(authorizeAttributeValue);
    }

    [Fact]
    public void Index_ShouldReturnView()
    {
        var result = _searchController.Index();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Results_ShouldSearchAppUsers()
    {
        AppUser user = new()
        {
            Id = "userId"
        };
        _appUserQueriesMock.Setup(u => u.SearchAppUsers("userId")).ReturnsAsync(new List<AppUser> { user });

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
        _appUserQueriesMock.Setup(u => u.SearchAppUsers("userId")).ReturnsAsync(new List<AppUser> { user });

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
        _appUserQueriesMock.Setup(u => u.SearchTweeds("term")).ReturnsAsync(new List<Data.Model.Tweed>
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
