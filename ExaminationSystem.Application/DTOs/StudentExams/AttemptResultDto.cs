namespace ExaminationSystem.Application.DTOs.StudentExams;

public class AttemptResultDto
{
    public double CurrentGrade { get; set; }
    public double MaxGrade { get; set; }
    public bool IsPassed { get; set; }
    public string CompletionTime { get; set; } = string.Empty;
}
