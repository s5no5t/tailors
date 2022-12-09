using Raven.Client.Documents.Indexes;

namespace Tweed.Data;

public class Tweeds_ByAuthorId : AbstractIndexCreationTask<Entities.Tweed>
{
    public Tweeds_ByAuthorId()
    {
        Map = tweeds => from tweed in tweeds
            select new
            {
                tweed.AuthorId,
                tweed.CreatedAt
            };
    }
}