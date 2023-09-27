using Tailors.User.Domain.UserFollowsAggregate;
using Xunit;

namespace Tailors.User.Test.Domain;

public class UserFollowsTest
{
    [Fact]
    public void BuildId_ShouldReturnCombinedId()
    {
        var userId = "User/123-A";
        
        var followsId = UserFollows.BuildId(userId);
        
        Assert.Equal($"{userId}/Follows", followsId);
    } 
}
