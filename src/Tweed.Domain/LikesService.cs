using NodaTime;
using Tweed.Domain.Model;

namespace Tweed.Domain;

public interface ITweedLikesService
{
    Task AddLike(string tweedId, string userId, ZonedDateTime likedAt);
    Task RemoveLike(string tweedId, string userId);
}

public class TweedLikesService : ITweedLikesService
{
    private readonly ITweedLikesRepository _tweedLikesRepository;

    public TweedLikesService(ITweedLikesRepository tweedLikesRepository)
    {
        _tweedLikesRepository = tweedLikesRepository;
    }

    public async Task AddLike(string tweedId, string userId, ZonedDateTime likedAt)
    {
        var appUserLikes = await GetOrBuildAppUserLikes(userId);

        if (appUserLikes.Likes.Any(l => l.TweedId == tweedId))
            return;
        appUserLikes.Likes.Add(new AppUserLikes.TweedLike
        {
            TweedId = tweedId,
            CreatedAt = likedAt
        });

        _tweedLikesRepository.IncreaseLikesCounter(tweedId);
    }

    public async Task RemoveLike(string tweedId, string userId)
    {
        var appUserLikes = await GetOrBuildAppUserLikes(userId);
        appUserLikes.Likes.RemoveAll(lb => lb.TweedId == tweedId);

        _tweedLikesRepository.DecreaseLikesCounter(tweedId);
    }

    private async Task<AppUserLikes> GetOrBuildAppUserLikes(string userId)
    {
        var appUserLikes = await _tweedLikesRepository.Get(userId);
        if (appUserLikes is null)
        {
            appUserLikes = new AppUserLikes
            {
                AppUserId = userId
            };
            await _tweedLikesRepository.Create(appUserLikes);
        }

        return appUserLikes;
    }
}
