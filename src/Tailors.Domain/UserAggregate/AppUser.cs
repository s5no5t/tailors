namespace Tailors.Domain.UserAggregate;

public class AppUser(string userName, long githubId, string? email = null, string? id = null)
{
    public string UserName { get; set; } = userName;
    public string? Id { get; set; } = id;
    public long GithubId { get; } = githubId;
    public string? Email { get; } = email;

    public static AppUser FromGithub(long githubId, string githubUsername)
    {
        return new AppUser(githubUsername, githubId);
    }
}
