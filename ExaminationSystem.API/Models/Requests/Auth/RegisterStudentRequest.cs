namespace ExaminationSystem.API.Models.Requests.Auth;

public class RegisterStudentRequest : RegisterUserBaseRequest
{
    // Student data
    public string Level { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
}
