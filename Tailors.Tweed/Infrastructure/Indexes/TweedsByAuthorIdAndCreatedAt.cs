using Raven.Client.Documents.Indexes;
using Tailors.Tweed.Domain;

namespace Tailors.Tweed.Infrastructure.Indexes;

public class TweedsByAuthorIdAndCreatedAt : AbstractIndexCreationTask<TailorsTweed>
{
    public TweedsByAuthorIdAndCreatedAt()
    {
        Map = tweeds => from tweed in tweeds
            select new
            {
                tweed.AuthorId,
                tweed.CreatedAt
            };
    }
}