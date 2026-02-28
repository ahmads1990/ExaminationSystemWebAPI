using ExaminationSystem.Domain.Entities;

namespace ExaminationSystem.Application.DTOs.Exams;

public class ListExamsDto : BasePaginatedDto
{
    public static readonly IReadOnlyList<string> AllowedSortFields =
        [nameof(Exam.DeadlineDate)];

    public string? Title { get; set; }
    public ExamType? ExamType { get; set; }
}
