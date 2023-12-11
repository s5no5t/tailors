using OneOf;
using OneOf.Types;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Tailors.Domain.UserAggregate;
using Tailors.Infrastructure.UserFollowsAggregate.Indexes;

namespace Tailors.Infrastructure.UserAggregate;

public class UserRepository(IAsyncDocumentSession session) : IUserRepository
{
    public async Task<List<AppUser>> Search(string term)
    {
        return await session.Query<AppUser, UsersByUserName>()
            .Search(u => u.UserName, $"{term}*")
            .Take(20).ToListAsync();
    }

    public async Task<OneOf<AppUser, None>> GetById(string tweedAuthorId)
    {
        var appUser = await session.LoadAsync<AppUser>(tweedAuthorId);
        if (appUser is not null)
            return appUser;
        return new None();
    }
}
