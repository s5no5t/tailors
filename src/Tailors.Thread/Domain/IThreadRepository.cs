namespace Tailors.Thread.Domain;

public interface IThreadRepository
{
    Task<TailorsThread?> GetById(string threadId);
    Task Create(TailorsThread thread);
}