using System.ComponentModel.DataAnnotations;

namespace Tweed.Web.Models;

public class Tweed
{
    [Required] [StringLength(280)] public string? Content { get; set; }
}
