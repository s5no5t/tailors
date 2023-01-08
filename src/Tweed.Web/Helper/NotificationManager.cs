namespace Tweed.Web.Helper;

public interface INotificationManager
{
    string? SuccessMessage { get; }
    void AppendSuccess(string message);
    void Clear();
}

public sealed class NotificationManager : INotificationManager
{
    private const string NotificationSuccessCookieName = "notification-success";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public NotificationManager(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? SuccessMessage
    {
        get
        {
            _httpContextAccessor.HttpContext!.Request.Cookies.TryGetValue(
                NotificationSuccessCookieName,
                out var successMessage);
            return successMessage;
        }
    }

    public void AppendSuccess(string message)
    {
        _httpContextAccessor.HttpContext!.Response.Cookies.Append(NotificationSuccessCookieName,
            message);
    }

    public void Clear()
    {
        _httpContextAccessor.HttpContext!.Response.Cookies.Delete(NotificationSuccessCookieName);
    }
}
