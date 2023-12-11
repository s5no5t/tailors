using JetBrains.Annotations;
using OneOf;
using OneOf.Types;

namespace Tailors.Domain.UserAggregate;

public interface IUserRepository
{
    [MustUseReturnValue]
    Task<List<AppUser>> Search(string term);

    Task<OneOf<AppUser, None>> GetById(string tweedAuthorId);
}
