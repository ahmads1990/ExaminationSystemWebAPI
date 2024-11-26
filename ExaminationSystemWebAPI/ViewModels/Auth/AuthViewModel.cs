namespace ExaminationSystemWebAPI.ViewModels.Auth;

public class AuthViewModel
{
    public string UserID { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsAuthenticated { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Claims { get; set; } = default!;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresOn { get; set; }
}
