namespace Tweed.Thread.Domain;

public interface ITweedThreadRepository
{
    Task<TweedThread?> GetById(string threadId);
    Task Create(TweedThread thread);
}