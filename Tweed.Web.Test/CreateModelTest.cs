using Tweed.Web.Pages;
using Xunit;

namespace Tweed.Web.Test;

public class CreateModelTest
{
    [Fact]
    public void OnPost()
    {
        var createModel = new CreateModel();
        createModel.OnPost();
    }
}
