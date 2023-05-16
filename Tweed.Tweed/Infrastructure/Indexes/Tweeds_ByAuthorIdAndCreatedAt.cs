using Raven.Client.Documents.Indexes;
using Tweed.Tweed.Domain;

namespace Tweed.Tweed.Infrastructure.Indexes;

public class Tweeds_ByAuthorIdAndCreatedAt : AbstractIndexCreationTask<TheTweed>
{
    public Tweeds_ByAuthorIdAndCreatedAt()
    {
        Map = tweeds => from tweed in tweeds
            select new
            {
                tweed.AuthorId,
                tweed.CreatedAt
            };
    }
}