using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tweed.Web.Test.TestHelper;
using Tweed.Web.Views.Tweed;
using Xunit;

namespace Tweed.Web.Test.Views;

public class CreateReplyTweedViewModelTest
{
    [Fact]
    public void ValidatesParentTweedId()
    {
        CreateReplyTweedViewModel createReplyTweedViewModel = new();

        var result = createReplyTweedViewModel.Validate();

        Assert.Equal(ModelValidationState.Invalid, result["parentTweedId"]!.ValidationState);
    }

    [Fact]
    public void ValidatesText()
    {
        CreateReplyTweedViewModel createReplyTweedViewModel = new();

        var result = createReplyTweedViewModel.Validate();

        Assert.Equal(ModelValidationState.Invalid, result["text"]!.ValidationState);
    }
}
