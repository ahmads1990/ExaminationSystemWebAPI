namespace ExaminationSystem.Application.DTOs.Auth;

public class RegisterInstructorDto
{
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}