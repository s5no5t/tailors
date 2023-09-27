using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Tailors.Domain.UserAggregate;
using Tailors.Infrastructure.UserFollowsAggregate.Indexes;

namespace Tailors.Infrastructure.UserAggregate;

public class UserRepository : IUserRepository
{
    private readonly IAsyncDocumentSession _session;

    public UserRepository(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task<List<AppUser>> Search(string term)
    {
        return await _session.Query<AppUser, UsersByUserName>()
            .Search(u => u.UserName, $"{term}*")
            .Take(20).ToListAsync();
    }
}
