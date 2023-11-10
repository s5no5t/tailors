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
        CreateReplyTweedViewModel sut = new();

        var result = sut.Validate();

        Assert.Equal(ModelValidationState.Invalid, result["parentTweedId"]!.ValidationState);
    }

    [Fact]
    public void ValidatesText()
    {
        CreateReplyTweedViewModel sut = new();

        var result = sut.Validate();

        Assert.Equal(ModelValidationState.Invalid, result["text"]!.ValidationState);
    }
}
