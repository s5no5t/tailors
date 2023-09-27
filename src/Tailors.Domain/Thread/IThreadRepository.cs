namespace Tailors.Domain.Thread;

public interface IThreadRepository
{
    Task<TailorsThread?> GetById(string threadId);
    Task Create(TailorsThread thread);
}