using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tailors.Web.Features.Tweed;
using Tailors.Web.Test.TestHelper;
using Xunit;

namespace Tailors.Web.Test.Views;

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
