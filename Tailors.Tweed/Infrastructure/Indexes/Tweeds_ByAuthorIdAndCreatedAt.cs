using Raven.Client.Documents.Indexes;

namespace Tailors.Tweed.Infrastructure.Indexes;

public class Tweeds_ByAuthorIdAndCreatedAt : AbstractIndexCreationTask<Domain.Tweed>
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