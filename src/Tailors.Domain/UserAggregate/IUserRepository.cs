using JetBrains.Annotations;

namespace Tailors.Domain.UserAggregate;

public interface IUserRepository
{
    [MustUseReturnValue]
    Task<List<AppUser>> Search(string term);

    [MustUseReturnValue]
    Task<AppUser?> GetById(string id);

    Task Create(AppUser user);

    [MustUseReturnValue]
    Task<AppUser?> FindByEmail(string email);
}
