using Microsoft.AspNetCore.Http;
using Moq;
using Tailors.Web.Helper;
using Xunit;

namespace Tailors.Web.Test.Helper;

public class NotificationManagerTest
{
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<IRequestCookieCollection> _requestCookiesMock;
    private readonly Mock<IResponseCookies> _responseCookiesMock;

    public NotificationManagerTest()
    {
        _requestCookiesMock = new Mock<IRequestCookieCollection>();
        _responseCookiesMock = new Mock<IResponseCookies>();

        var requestMock = new Mock<HttpRequest>();
        requestMock.Setup(r => r.Cookies).Returns(_requestCookiesMock.Object);

        var responseMock = new Mock<HttpResponse>();
        responseMock.Setup(r => r.Cookies).Returns(_responseCookiesMock.Object);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.Response).Returns(responseMock.Object);
        httpContextMock.Setup(c => c.Request).Returns(requestMock.Object);

        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockHttpContextAccessor.Setup(c => c.HttpContext).Returns(httpContextMock.Object);
        _mockHttpContextAccessor.Setup(c => c.HttpContext).Returns(httpContextMock.Object);
    }

    [Fact]
    public void AppendSuccess_ShouldAppendCookie()
    {
        var sut = new NotificationManager(_mockHttpContextAccessor.Object);

        sut.AppendSuccess("success");

        _responseCookiesMock.Verify(c => c.Append("notification-success", "success"));
    }

    [Fact]
    public void SuccessMessage_ShouldReturnMessage_WhenCookieIsSet()
    {
        var value = "success";
        _requestCookiesMock.Setup(r => r.TryGetValue("notification-success", out value));

        var sut = new NotificationManager(_mockHttpContextAccessor.Object);

        Assert.Equal("success", sut.SuccessMessage);
    }

    [Fact]
    public void SuccessMessage_ShouldReturnNull_WhenNoCookieIsSet()
    {
        var sut = new NotificationManager(_mockHttpContextAccessor.Object);

        Assert.Null(sut.SuccessMessage);
    }

    [Fact]
    public void Clear_ShouldDeleteCookie()
    {
        var sut = new NotificationManager(_mockHttpContextAccessor.Object);

        sut.Clear();

        _responseCookiesMock.Verify(c => c.Delete("notification-success"));
    }
}
