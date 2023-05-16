using Tweed.Domain.Model;

namespace Tweed.Domain;

public interface ITweedThreadRepository
{
    Task<TweedThread?> GetById(string threadId);
    Task Create(TweedThread thread);
}