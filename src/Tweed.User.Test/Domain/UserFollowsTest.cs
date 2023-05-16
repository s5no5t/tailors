using Tweed.User.Domain;
using Xunit;

namespace Tweed.User.Test.Domain;

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
