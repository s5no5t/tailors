using OneOf;
using OneOf.Types;

namespace Tailors.Domain.UserFollowsAggregate;

public class UserFollowsRepositoryMock : IUserFollowsRepository
{
    private readonly Dictionary<string, UserFollows> _userFollows = new();

    public Task<OneOf<UserFollows, None>> GetById(string userFollowsId)
    {
        _userFollows.TryGetValue(userFollowsId, out var userFollows);

        if (userFollows is not null)
            return Task.FromResult<OneOf<UserFollows, None>>(userFollows);

        return Task.FromResult<OneOf<UserFollows, None>>(new None());
    }

    public Task Create(UserFollows userFollows)
    {
        userFollows.Id = UserFollows.BuildId(userFollows.UserId);
        _userFollows.Add(userFollows.Id, userFollows);
        return Task.CompletedTask;
    }

    public Task<int> GetFollowerCount(string userId)
    {
        throw new NotImplementedException();
    }
}
