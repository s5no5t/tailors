using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tweed.Web.Controllers;
using Xunit;

namespace Tweed.Web.Test.Controllers;

public class SearchControllerTest
{
    private readonly SearchController _searchController;

    public SearchControllerTest()
    {
        _searchController = new SearchController();
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
}

