namespace Tailors.Domain.User;

public interface IUserRepository
{
    Task<List<AppUser>> Search(string term);
}