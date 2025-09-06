namespace ExaminationSystem.API.Models.Requests.Auth;

public class RegisterInstructorRequest: RegisterUserBaseRequest
{
    // Instructor data
    public string Bio { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
}
