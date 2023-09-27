namespace Tailors.Domain.ThreadAggregate;

public interface IThreadRepository
{
    Task<TailorsThread?> GetById(string threadId);
    Task Create(TailorsThread thread);
}