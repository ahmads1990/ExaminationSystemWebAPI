namespace ExaminationSystem.Application.DTOs.StudentExams;

public class AttemptSummaryDto
{
    public string CourseName { get; set; } = string.Empty;
    public string ExamTitle { get; set; } = string.Empty;
    public ExamType ExamType { get; set; }
    public double Grade { get; set; }
    public double MaxGrade { get; set; }
    public ExamAttemptStatus Status { get; set; }
    public string CompletionTime { get; set; } = string.Empty;
    public DateTime CreateDate { get; set; }
}
