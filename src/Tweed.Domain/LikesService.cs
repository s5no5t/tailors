using NodaTime;
using Tweed.Domain.Model;

namespace Tweed.Domain;

public interface ILikesService
{
    Task AddLike(string tweedId, string userId, ZonedDateTime likedAt);
    Task RemoveLike(string tweedId, string userId);
    Task<bool> DoesUserLikeTweed(string tweedId, string userId);
}

public class LikesService : ILikesService
{
    private readonly ITweedLikesRepository _tweedLikesRepository;

    public LikesService(ITweedLikesRepository tweedLikesRepository)
    {
        _tweedLikesRepository = tweedLikesRepository;
    }

    public async Task AddLike(string tweedId, string userId, ZonedDateTime likedAt)
    {
        var appUserLikes = await GetOrCreateAppUserLikes(userId);

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
        var appUserLikes = await GetOrCreateAppUserLikes(userId);
        appUserLikes.Likes.RemoveAll(lb => lb.TweedId == tweedId);

        _tweedLikesRepository.DecreaseLikesCounter(tweedId);
    }

    public async Task<bool> DoesUserLikeTweed(string tweedId, string userId)
    {
        var appUserLikes = await GetOrCreateAppUserLikes(userId);
        return appUserLikes.Likes.Any(lb => lb.TweedId == tweedId);
    }

    private async Task<AppUserLikes> GetOrCreateAppUserLikes(string userId)
    {
        var appUserLikesId = AppUserLikes.BuildId(userId);
        var appUserLikes = await _tweedLikesRepository.GetById(appUserLikesId);
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
