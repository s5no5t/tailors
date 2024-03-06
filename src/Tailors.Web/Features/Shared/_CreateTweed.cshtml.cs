using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Tailors.Web.Features.Shared;

public class CreateTweedViewModel
{
    [BindProperty]
    [Required]
    [StringLength(280)]
    public string Text { get; set; } = "";

    [BindProperty] public string? ParentTweedId { get; set; }
}
