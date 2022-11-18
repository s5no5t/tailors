using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Raven.Client.Documents.Session;
using Xunit;

namespace Tweed.Web.Test;

public class SaveChangesMiddlewareTest
{
    [Fact]
    public async Task InvokeAsync()
    {
        var sessionMock = new Mock<IAsyncDocumentSession>();

        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services => { services.AddSingleton(sessionMock.Object); })
                    .Configure(app => { app.UseMiddleware<SaveChangesMiddleware>(); });
            })
            .StartAsync();

        await host.GetTestClient().GetAsync("/");

        sessionMock.Verify(s => s.SaveChangesAsync(default));
    }
}
