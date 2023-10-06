using JetBrains.Annotations;
using OneOf;
using OneOf.Types;

namespace Tailors.Domain.ThreadAggregate;

public interface IThreadRepository
{
    [MustUseReturnValue]
    Task<OneOf<TailorsThread, None>> GetById(string threadId);

    Task Create(TailorsThread thread);
}
