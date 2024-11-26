using System.ComponentModel.DataAnnotations;

namespace ExaminationSystemWebAPI.ViewModels.Auth;

public class AddClaimViewModel
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string ClaimType { get; set; } = string.Empty;
    [Required]
    public string ClaimValue { get; set; } = string.Empty;
}
