namespace Tweed.Like.Domain;

public interface ITweedLikesRepository
{
    Task<UserLikes?> GetById(string userLikesId);
    Task Create(UserLikes userLikes);
    Task<long> GetLikesCounter(string tweedId);
    void IncreaseLikesCounter(string tweedId);
    void DecreaseLikesCounter(string tweedId);
}