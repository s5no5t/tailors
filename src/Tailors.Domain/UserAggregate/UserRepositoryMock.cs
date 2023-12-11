using OneOf;
using OneOf.Types;

namespace Tailors.Domain.UserAggregate;

public class UserRepositoryMock : IUserRepository
{
    private readonly Random _random = new();
    private readonly Dictionary<string, AppUser> _users = new();

    public Task<List<AppUser>> Search(string term)
    {
        var users = _users.Values.Where(u => u.UserName.Contains(term)).ToList();
        return Task.FromResult(users);
    }

    public Task<OneOf<AppUser, None>> GetById(string tweedAuthorId)
    {
        throw new NotImplementedException();
    }

    public Task Create(AppUser appUser)
    {
        appUser.Id ??= _random.Next().ToString();
        _users.Add(appUser.Id, appUser);
        return Task.CompletedTask;
    }
}
