using Moq;

namespace Tailors.Thread.Test.Domain;

public static class MoqExtensions
{
    public static IEnumerable<T> CollectionMatcher<T>(IEnumerable<T> expectations)
    {
        var expectationsList = expectations.ToList();

        return Match.Create((IEnumerable<T> inputCollection) =>
        {
            var input = inputCollection.ToList();
            return expectationsList.Except(input).Any() &&
                   !input.Except(expectationsList).Any();
        });
    }
}