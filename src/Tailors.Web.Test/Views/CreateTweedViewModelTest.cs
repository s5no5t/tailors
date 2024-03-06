using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tailors.Web.Features.Shared;
using Tailors.Web.Features.Tweed;
using Tailors.Web.Test.TestHelper;
using Xunit;

namespace Tailors.Web.Test.Views;

public class CreateTweedViewModelTest
{
    [Fact]
    public void ValidatesTextRequired()
    {
        CreateTweedViewModel sut = new();

        var result = sut.Validate();

        Assert.Equal(ModelValidationState.Invalid, result["text"]!.ValidationState);
    }

    [Fact]
    public void ValidatesTextTooLong()
    {
        CreateTweedViewModel sut = new()
        {
            Text = new string('a', 281)
        };

        var result = sut.Validate();

        Assert.Equal(ModelValidationState.Invalid, result["text"]!.ValidationState);
    }
}
