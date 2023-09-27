using Raven.Client.Documents.Indexes;
using Tailors.Domain.TweedAggregate;

namespace Tailors.Infrastructure.TweedAggregate.Indexes;

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