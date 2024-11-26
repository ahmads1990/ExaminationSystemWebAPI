using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ExaminationSystemWebAPI.ViewModels.Auth.Register;

public class RegisterViewModel
{
    [Required]
    [StringLength(20, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [JsonIgnore]
    public string ClaimRole { get; set; } = string.Empty;
}