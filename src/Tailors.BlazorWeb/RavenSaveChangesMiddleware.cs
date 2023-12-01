using Raven.Client.Documents.Session;

namespace Tailors.BlazorWeb;

public class RavenSaveChangesMiddleware
{
    private readonly RequestDelegate _next;

    // ReSharper disable once ConvertToPrimaryConstructor
    public RavenSaveChangesMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAsyncDocumentSession documentSession)
    {
        await _next(context);
        await documentSession.SaveChangesAsync();
    }
}
