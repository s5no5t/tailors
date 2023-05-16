using Tweed.Domain.Model;
using Xunit;

namespace Tweed.Domain.Test.Model;

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
