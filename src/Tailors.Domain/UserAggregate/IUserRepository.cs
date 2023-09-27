namespace Tailors.Domain.UserAggregate;

public interface IUserRepository
{
    Task<List<AppUser>> Search(string term);
}