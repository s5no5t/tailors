namespace Tailors.Domain.UserAggregate;

public class AppUser(string userName, string email, string? id = null)
{
    public string UserName { get; set; } = userName;
    public string? Id { get; set; } = id;
    public string Email { get; } = email;
}
