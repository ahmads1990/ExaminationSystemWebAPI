namespace ExaminationSystem.Application.DTOs.Auth;

public class CreateExamTokenDto
{
    public int StudentId { get; set; }
    public int ExamAttemptId { get; set; }
    public int MaxDurationInMinutes { get; set; }
}
