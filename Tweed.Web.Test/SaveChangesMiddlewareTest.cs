using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Tweed.Data.Test;
using Xunit;

namespace Tweed.Web.Test;

public class SaveChangesMiddlewareTest
{
    [Fact]
    public async Task InvokeAsync()
    {
        using var ravenDb = new RavenTestDb();
        using var session = ravenDb.Session;

        Data.Models.Tweed tweed = new()
        {
            Content = "test"
        };
        await session.StoreAsync(tweed);

        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton(ravenDb);
                        services.AddSingleton(session);
                    })
                    .Configure(app => { app.UseMiddleware<SaveChangesMiddleware>(); });
            })
            .StartAsync();

        await host.GetTestClient().GetAsync("/");

        var data = await session.Query<Data.Models.Tweed>().Where(t => t.Content == "test").FirstAsync();
        Assert.NotNull(data);
    }
}
