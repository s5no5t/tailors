using OneOf;
using OneOf.Types;

namespace Tailors.Domain.UserFollowsAggregate;

public interface IUserFollowsRepository
{
    Task<OneOf<UserFollows, None>> GetById(string userFollowsId);
    Task Create(UserFollows userFollows);
    Task<int> GetFollowerCount(string userId);
}