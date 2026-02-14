namespace ExaminationSystem.API.Models.Requests.Auth;

public class RefreshTokenRequest
{
    public int UserId { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
}
