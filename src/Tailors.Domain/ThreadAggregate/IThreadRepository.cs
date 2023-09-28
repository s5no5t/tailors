using OneOf;
using OneOf.Types;

namespace Tailors.Domain.ThreadAggregate;

public interface IThreadRepository
{
    Task<OneOf<TailorsThread, None>> GetById(string threadId);
    Task Create(TailorsThread thread);
}