using OneOf;
using OneOf.Types;
using Tailors.Domain.UserLikesAggregate;

namespace Tailors.Domain.Test.LikeAggregate;

public class UserLikesRepositoryMock : IUserLikesRepository
{
    private readonly Dictionary<string, long> _likesCounter = new();
    private readonly Dictionary<string, UserLikes> _userLikes = new();

    public Task<OneOf<UserLikes, None>> GetById(string userLikesId)
    {
        _userLikes.TryGetValue(userLikesId, out var userLikes);

        if (userLikes is not null)
            return Task.FromResult<OneOf<UserLikes, None>>(userLikes);

        return Task.FromResult<OneOf<UserLikes, None>>(new None());
    }

    public Task Create(UserLikes userLikes)
    {
        userLikes.Id = UserLikes.BuildId(userLikes.UserId);

        _userLikes.Add(userLikes.Id!, userLikes);
        return Task.CompletedTask;
    }

    public Task<long> GetLikesCounter(string tweedId)
    {
        _likesCounter.TryGetValue(tweedId, out var likesCounter);

        return Task.FromResult(likesCounter);
    }

    public void IncreaseLikesCounter(string tweedId)
    {
        _likesCounter.TryGetValue(tweedId, out var likesCounter);
        _likesCounter[tweedId] = likesCounter + 1;
    }

    public void DecreaseLikesCounter(string tweedId)
    {
        _likesCounter.TryGetValue(tweedId, out var likesCounter);
        _likesCounter[tweedId] = likesCounter - 1;
    }
}
