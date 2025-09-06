namespace ExaminationSystem.Application.DTOs.Auth;

public class RegisterStudentDto
{
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
}
