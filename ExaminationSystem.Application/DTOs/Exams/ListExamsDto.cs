using ExaminationSystem.Domain.Entities;

namespace ExaminationSystem.Application.DTOs.Exams;

public class ListExamsDto : BasePaginatedDto
{
    public static readonly IReadOnlyList<string> AllowedSortFields =
        [nameof(Exam.DeadlineDate), nameof(Exam.Title), nameof(Exam.TotalGrade)];

    public string? Title { get; set; }
    public ExamType? ExamType { get; set; }
    public ExamStatus? ExamStatus { get; set; }
    public int? CourseId { get; set; }
    public int? InstructorId { get; set; }
    public DateTime? DeadlineFrom { get; set; }
    public DateTime? DeadlineTo { get; set; }
}
