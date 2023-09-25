namespace Tailors.Thread.Domain.ThreadAggregate;

public interface ITweedThreadRepository
{
    Task<TweedThread?> GetById(string threadId);
    Task Create(TweedThread thread);
}