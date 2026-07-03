using ExaminationSystem.Application.DTOs.StudentExams;
using ExaminationSystem.Domain.Entities;

namespace ExaminationSystem.Application.DTOs.Instructor;

/// <summary>
/// Data Transfer Object for listing and filtering exam submissions.
/// </summary>
public class ListExamSubmissionsDto : BasePaginatedDto
{
    /// <summary>
    /// The allowed sorting fields.
    /// </summary>
    public static readonly IReadOnlyList<string> AllowedSortFields =
        [nameof(AttemptSummaryDto.StudentName), nameof(AttemptSummaryDto.Grade), nameof(AttemptSummaryDto.CreateDate), nameof(AttemptSummaryDto.Status)];

    /// <summary>
    /// Search filter for the student's name.
    /// </summary>
    public string? StudentName { get; set; }

    /// <summary>
    /// Filter for the status of the exam attempt.
    /// </summary>
    public ExamAttemptStatus? Status { get; set; }
}
