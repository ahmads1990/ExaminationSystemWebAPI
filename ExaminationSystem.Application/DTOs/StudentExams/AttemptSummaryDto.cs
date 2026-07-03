namespace ExaminationSystem.Application.DTOs.StudentExams;

/// <summary>
/// Data Transfer Object representing an attempt summary.
/// </summary>
public class AttemptSummaryDto
{
    /// <summary>
    /// Gets or sets the student's unique identifier.
    /// </summary>
    public int StudentId { get; set; }

    /// <summary>
    /// Gets or sets the student's name.
    /// </summary>
    public string StudentName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the course name.
    /// </summary>
    public string CourseName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the exam title.
    /// </summary>
    public string ExamTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the exam type.
    /// </summary>
    public ExamType ExamType { get; set; }

    /// <summary>
    /// Gets or sets the grade achieved.
    /// </summary>
    public double Grade { get; set; }

    /// <summary>
    /// Gets or sets the maximum possible grade.
    /// </summary>
    public double MaxGrade { get; set; }

    /// <summary>
    /// Gets or sets the status of the exam attempt.
    /// </summary>
    public ExamAttemptStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the formatted completion time of the exam attempt.
    /// </summary>
    public string CompletionTime { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation date/start time of the exam attempt.
    /// </summary>
    public DateTime CreateDate { get; set; }
}

