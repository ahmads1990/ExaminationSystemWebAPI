namespace ExaminationSystem.API.Models.Requests.Auth;

public class RegisterInstructorRequest
{
    // User data
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    // Instructor data
    public string Bio { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
}
