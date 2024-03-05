namespace Tailors.Domain.UserAggregate;

public class AuthenticationUseCase(IUserRepository userRepository)
{
    public async Task<AppUser> EnsureUserExists(string email, string userName)
    {
        var user = await userRepository.FindByEmail(email);
        if (user is null)
        {
            user = new AppUser(userName, email);
            await userRepository.Create(user);
        }

        return user;
    }
}
