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

    public async Task<AppUser?> GetById(string id)
    {
        return await session.LoadAsync<AppUser>(id)!;
    }

    public async Task Create(AppUser user)
    {
        await session.StoreAsync(user);
    }

    public async Task<AppUser?> FindByEmail(string email)
    {
        return await session.Query<AppUser>()
            .Where(u => u.Email == email)
            .FirstOrDefaultAsync();
    }
}
