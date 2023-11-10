using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tailors.Domain.TweedAggregate;
using Tailors.Domain.UserAggregate;
using Tailors.Web.Features.Search;
using Xunit;

namespace Tailors.Web.Test.Controllers;

public class SearchControllerTest
{
    private readonly SearchController _sut = new();
    private readonly TweedRepositoryMock _tweedRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();

    public SearchControllerTest()
    {
        _userRepositoryMock.Setup(u => u.Search(It.IsAny<string>()))
            .ReturnsAsync(new List<AppUser>());
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
        var result = _sut.Index();

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

        var result = await _sut.Results("userId", _tweedRepositoryMock, _userRepositoryMock.Object);

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

        var result = await _sut.Results("userId", _tweedRepositoryMock, _userRepositoryMock.Object);

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<IndexViewModel>(resultAsView.Model);
        var viewModel = (IndexViewModel)resultAsView.Model!;
        Assert.Equal("UserName", viewModel.FoundUsers[0].UserName);
    }

    [Fact]
    public async Task Results_ShouldSearchTweeds()
    {
        await _tweedRepositoryMock.Create(new Tweed(id: "tweedId", authorId: "authorId", text: "term",
            createdAt: DateTime.Now));

        var result = await _sut.Results("term", _tweedRepositoryMock, _userRepositoryMock.Object);

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<IndexViewModel>(resultAsView.Model);
        var viewModel = (IndexViewModel)resultAsView.Model!;
        Assert.Contains(viewModel.FoundTweeds, t => t.TweedId == "tweedId");
    }
}
