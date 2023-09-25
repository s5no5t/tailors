using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tailors.Thread.Domain;
using Tailors.Thread.Infrastructure;
using Tailors.User.Domain;
using Tailors.Web.Features.Search;
using Xunit;

namespace Tailors.Web.Test.Controllers;

public class SearchControllerTest
{
    private readonly SearchController _searchController = new();
    private readonly Mock<ITweedRepository> _tweedRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();

    public SearchControllerTest()
    {
        _userRepositoryMock.Setup(u => u.Search(It.IsAny<string>()))
            .ReturnsAsync(new List<AppUser>());
        _tweedRepositoryMock.Setup(u => u.Search(It.IsAny<string>()))
            .ReturnsAsync(new List<Tweed>());
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
    public async Task Results_ShouldSearchUsers()
    {
        AppUser user = new()
        {
            Id = "userId"
        };
        _userRepositoryMock.Setup(u => u.Search("userId"))
            .ReturnsAsync(new List<AppUser> { user });

        var result = await _searchController.Results("userId", _tweedRepositoryMock.Object, _userRepositoryMock.Object);

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
        _userRepositoryMock.Setup(u => u.Search("userId"))
            .ReturnsAsync(new List<AppUser> { user });

        var result = await _searchController.Results("userId", _tweedRepositoryMock.Object, _userRepositoryMock.Object);

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<IndexViewModel>(resultAsView.Model);
        var viewModel = (IndexViewModel)resultAsView.Model!;
        Assert.Equal("UserName", viewModel.FoundUsers[0].UserName);
    }

    [Fact]
    public async Task Results_ShouldSearchTweeds()
    {
        _tweedRepositoryMock.Setup(u => u.Search("term")).ReturnsAsync(
            new List<Tweed>
            {
                new()
                {
                    Id = "tweedId"
                }
            });

        var result = await _searchController.Results("term", _tweedRepositoryMock.Object, _userRepositoryMock.Object);

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<IndexViewModel>(resultAsView.Model);
        var viewModel = (IndexViewModel)resultAsView.Model!;
        Assert.Contains(viewModel.FoundTweeds, t => t.TweedId == "tweedId");
    }
}
