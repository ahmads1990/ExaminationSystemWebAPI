using ExaminationSystem.Domain.Common;

namespace ExaminationSystem.Application.DTOs.StudentExams;

public class AvailableExamDto
{
    public int ExamId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public ExamType ExamType { get; set; }
    public DateTime DeadlineDate { get; set; }
    public int MaxDurationInMinutes { get; set; }
    public int MaxAttempts { get; set; }
    public int AttemptsTaken { get; set; }
}
