using JetBrains.Annotations;
using OneOf;
using OneOf.Types;

namespace Tailors.Domain.UserLikesAggregate;

public interface IUserLikesRepository
{
    [MustUseReturnValue]
    Task<OneOf<UserLikes, None>> GetById(string userLikesId);
    Task Create(UserLikes userLikes);
    [MustUseReturnValue]
    Task<long> GetLikesCounter(string tweedId);
    void IncreaseLikesCounter(string tweedId);
    void DecreaseLikesCounter(string tweedId);
}