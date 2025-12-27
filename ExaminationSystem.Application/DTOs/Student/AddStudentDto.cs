namespace ExaminationSystem.Application.DTOs.Student;

public class AddStudentDto
{
    public int ID { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
}
