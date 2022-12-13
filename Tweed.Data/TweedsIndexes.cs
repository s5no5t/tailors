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

public class Tweeds_ByText : AbstractIndexCreationTask<Entities.Tweed>
{
    public Tweeds_ByText()
    {
        Map = tweeds => from tweed in tweeds
            select new
            {
                tweed.Id,
                tweed.Text
            };

        Analyzers.Add(a => a.Text, "StandardAnalyzer");
    }
}
