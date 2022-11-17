using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tweed.Web.Pages;
using Xunit;

namespace Tweed.Web.Test;

public class CreateModelTest
{
    [Fact]
    public async Task OnPostAsync_InvalidModel_ReturnsPageResult()
    {
        var createModel = new CreateModel();
        createModel.ModelState.AddModelError("someKey", "errorMessage");
        var result = await createModel.OnPostAsync();
        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task OnPostAsync_ValidModel_ReturnsRedirectToPageResult()
    {
        var createModel = new CreateModel();
        var result = await createModel.OnPostAsync();
        Assert.IsType<RedirectToPageResult>(result);
    }
}
