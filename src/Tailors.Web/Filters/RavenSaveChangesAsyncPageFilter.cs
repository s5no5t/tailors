using Microsoft.AspNetCore.Mvc.Filters;
using Raven.Client.Documents.Session;

namespace Tailors.Web.Filters;

public class RavenSaveChangesAsyncPageFilter : IAsyncPageFilter
{
    private readonly IAsyncDocumentSession _dbSession;

    public RavenSaveChangesAsyncPageFilter(IAsyncDocumentSession dbSession)
    {
        _dbSession = dbSession;
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        return Task.CompletedTask;
    }

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
    {
        await next();

        await _dbSession.SaveChangesAsync();
    }
}
