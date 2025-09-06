namespace ExaminationSystem.Application.DTOs.Student;

public class AddStudentDto
{
    public int AppUserId { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
}
