using Microsoft.AspNetCore.Identity;
using Tailors.Domain.UserAggregate;

namespace Tailors.BlazorWeb.Components.Account;

internal sealed class IdentityUserAccessor(UserManager<AppUser> userManager, IdentityRedirectManager redirectManager)
{
    public async Task<AppUser> GetRequiredUserAsync(HttpContext context)
    {
        Console.WriteLine("IdentityUserAccessor Current Thread ID:" + Thread.CurrentThread.ManagedThreadId);
        var user = await userManager.GetUserAsync(context.User);

        if (user is null)
            redirectManager.RedirectToWithStatus("Account/InvalidUser",
                $"Error: Unable to load user with ID '{userManager.GetUserId(context.User)}'.", context);

        return user;
    }
}
