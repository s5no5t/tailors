using Moq;

namespace Tailors.Domain.Test.ThreadAggregate;

public class MoqExtensions
{
    public static IEnumerable<T> CollectionMatcher<T>(IEnumerable<T> expectation)
    {
        return Match.Create((IEnumerable<T> inputCollection) =>
            !expectation.Except(inputCollection).Any() &&
            !inputCollection.Except(expectation).Any());
    }
}