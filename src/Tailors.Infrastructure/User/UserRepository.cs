using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Tailors.Domain.AppUser;
using Tailors.User.Infrastructure.Indexes;

namespace Tailors.User.Infrastructure;

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