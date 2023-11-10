using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Tailors.Web.Test.TestHelper;

internal static class ViewModelValidator
{
    internal static ModelStateDictionary Validate(this object viewModel)
    {
        var validationContext = new ValidationContext(viewModel, null, null);
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(viewModel, validationContext, validationResults, true);
        ModelStateDictionary modelStateDictionary = new();
        foreach (var validationResult in validationResults)
            modelStateDictionary.AddModelError(
                validationResult.MemberNames.FirstOrDefault() ?? string.Empty,
                validationResult.ErrorMessage!);

        return modelStateDictionary;
    }

    internal static void ValidateViewModel(this ControllerBase controller, object viewModel)
    {
        var validationContext = new ValidationContext(viewModel, null, null);
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(viewModel, validationContext, validationResults, true);
        foreach (var validationResult in validationResults)
            controller.ModelState.AddModelError(
                validationResult.MemberNames.FirstOrDefault() ?? string.Empty,
                validationResult.ErrorMessage!);
    }
}
