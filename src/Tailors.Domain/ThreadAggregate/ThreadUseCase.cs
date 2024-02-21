using OneOf;
using Tailors.Domain.TweedAggregate;

namespace Tailors.Domain.ThreadAggregate;

public class ThreadUseCase(ITweedRepository tweedRepository)
{
    public async Task<OneOf<List<Tweed>, ResourceNotFoundError>> GetThreadTweedsForTweed(string tweedId)
    {
        var getTweedResult = await tweedRepository.GetById(tweedId);
        if (getTweedResult.TryPickT1(out _, out var tweed))
            return new ResourceNotFoundError($"Tweed {tweedId} not found");

        var tweedPath = tweed.LeadingTweedIds.ToList();
        tweedPath.Add(tweedId);
        var tweedsByIds = await tweedRepository.GetByIds(tweedPath);
        var tweeds = tweedsByIds.Values.Select(t => t).ToList();
        return tweeds;
    }
}
