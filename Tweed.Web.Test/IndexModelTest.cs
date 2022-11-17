using Microsoft.Extensions.Logging;
using Moq;
using Tweed.Web.Pages;
using Xunit;

namespace Tweed.Web.Test;

public class IndexModelTest
{
    [Fact]
    public void OnGet()
    {
        var loggerMock = new Mock<ILogger<IndexModel>>();
        var indexModel = new IndexModel(loggerMock.Object);
        indexModel.OnGet();
    }
}