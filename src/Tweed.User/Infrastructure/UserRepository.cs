using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Tweed.User.Domain;
using Tweed.User.Infrastructure.Indexes;

namespace Tweed.User.Infrastructure;

public class UserRepository : IUserRepository
{
    private readonly IAsyncDocumentSession _session;

    public UserRepository(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task<List<Domain.AppUser>> Search(string term)
    {
        return await _session.Query<Domain.AppUser, Users_ByUserName>()
            .Search(u => u.UserName, $"{term}*")
            .Take(20).ToListAsync();
    }
}
