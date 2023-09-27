namespace Tailors.Domain.Test.UserFollowsAggregate;

public class UserFollowsTest
{
    [Fact]
    public void BuildId_ShouldReturnCombinedId()
    {
        var userId = "User/123-A";
        
        var followsId = Domain.UserFollowsAggregate.UserFollows.BuildId(userId);
        
        Assert.Equal($"{userId}/Follows", followsId);
    } 
}
