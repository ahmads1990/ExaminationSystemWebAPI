namespace ExaminationSystem.Application.DTOs.Instructor;

public class AddInstructorDto
{
    public int ID { get; set; }
    public string Bio { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
}

