using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Tweed.Web.Features.Tweed;

public class CreateTweedViewModel
{
    [BindProperty]
    [Required]
    [StringLength(280)]
    public string Text { get; set; } = "";
}
