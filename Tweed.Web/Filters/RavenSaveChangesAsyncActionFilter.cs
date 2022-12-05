using Microsoft.AspNetCore.Mvc.Filters;
using Raven.Client.Documents.Session;

namespace Tweed.Web.Filters;

public class RavenSaveChangesAsyncActionFilter : IAsyncActionFilter
{
    private readonly IAsyncDocumentSession _dbSession;

    public RavenSaveChangesAsyncActionFilter(IAsyncDocumentSession dbSession)
    {
        _dbSession = dbSession;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        await next();

        await _dbSession.SaveChangesAsync();
    }
}
