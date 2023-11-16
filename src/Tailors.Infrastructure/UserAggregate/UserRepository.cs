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
}
