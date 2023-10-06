using Moq;

namespace Tailors.Domain.Test.ThreadAggregate;

public static class MoqExtensions
{
    public static IEnumerable<T> CollectionMatcher<T>(IEnumerable<T> expectation)
    {
        var expectationArray = expectation as T[] ?? expectation.ToArray();
        return Match.Create((IEnumerable<T> inputCollection) =>
        {
            var inputCollectionArray = inputCollection as T[] ?? inputCollection.ToArray();
            return !expectationArray.Except(inputCollectionArray).Any() &&
                   !inputCollectionArray.Except(expectationArray).Any();
        });
    }
}