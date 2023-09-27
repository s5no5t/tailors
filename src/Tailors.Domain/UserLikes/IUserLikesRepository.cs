namespace Tailors.Domain.UserLikes;

public interface IUserLikesRepository
{
    Task<UserLikes?> GetById(string userLikesId);
    Task Create(UserLikes userLikes);
    Task<long> GetLikesCounter(string tweedId);
    void IncreaseLikesCounter(string tweedId);
    void DecreaseLikesCounter(string tweedId);
}