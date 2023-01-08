namespace Tweed.Web.Areas.Identity;

public interface IEmailSender
{
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be
        ///     used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        Task SendEmailAsync(string email, string subject, string htmlMessage);
}
