namespace Tailors.Domain.Test.UserFollows;

public class UserFollowsTest
{
    [Fact]
    public void BuildId_ShouldReturnCombinedId()
    {
        var userId = "User/123-A";
        
        var followsId = Domain.UserFollows.UserFollows.BuildId(userId);
        
        Assert.Equal($"{userId}/Follows", followsId);
    } 
}
