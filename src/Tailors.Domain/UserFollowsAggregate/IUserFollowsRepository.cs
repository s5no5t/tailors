using JetBrains.Annotations;
using OneOf;
using OneOf.Types;

namespace Tailors.Domain.UserFollowsAggregate;

public interface IUserFollowsRepository
{
    [MustUseReturnValue]
    Task<OneOf<UserFollows, None>> GetById(string userFollowsId);

    Task Create(UserFollows userFollows);

    [MustUseReturnValue]
    Task<int> GetFollowerCount(string userId);
}
