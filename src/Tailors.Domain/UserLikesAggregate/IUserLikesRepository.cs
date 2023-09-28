using OneOf;
using OneOf.Types;

namespace Tailors.Domain.UserLikesAggregate;

public interface IUserLikesRepository
{
    Task<OneOf<UserLikes, None>> GetById(string userLikesId);
    Task Create(UserLikes userLikes);
    Task<long> GetLikesCounter(string tweedId);
    void IncreaseLikesCounter(string tweedId);
    void DecreaseLikesCounter(string tweedId);
}