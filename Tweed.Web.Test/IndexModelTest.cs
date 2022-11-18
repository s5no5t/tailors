using Tweed.Web.Pages;
using Xunit;

namespace Tweed.Web.Test;

public class IndexModelTest
{
    [Fact]
    public void OnGet()
    {
        var indexModel = new IndexModel();
        indexModel.OnGet();
    }
}
