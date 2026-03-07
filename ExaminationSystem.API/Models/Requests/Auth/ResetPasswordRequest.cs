namespace ExaminationSystem.API.Models.Requests.Auth;

public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string OTP { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
