using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Tweed.Domain;
using Tweed.Domain.Model;
using Tweed.Infrastructure.Indexes;

namespace Tweed.Infrastructure;

public class UserRepository : IUserRepository
{
    private readonly IAsyncDocumentSession _session;

    public UserRepository(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task<List<User>> Search(string term)
    {
        return await _session.Query<User, Users_ByUserName>()
            .Search(u => u.UserName, $"{term}*")
            .Take(20).ToListAsync();
    }
}
