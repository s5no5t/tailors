namespace Tailors.Domain.UserAggregate;

public class AuthenticationUseCase(IUserRepository userRepository)
{
    public async Task<AppUser> EnsureUserExistsForGithubUser(long githubId, string githubUsername)
    {
        var user = await userRepository.FindByGithubId(githubId);
        if (user is null)
        {
            user = AppUser.FromGithub(githubId, githubUsername);
            await userRepository.Create(user);
        }

        return user;
    }
}
