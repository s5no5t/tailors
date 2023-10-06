using JetBrains.Annotations;

namespace Tailors.Domain.UserAggregate;

public interface IUserRepository
{
    [MustUseReturnValue]
    Task<List<AppUser>> Search(string term);
}