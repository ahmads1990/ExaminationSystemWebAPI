using ExaminationSystem.Domain.Entities;

namespace ExaminationSystem.API.Models.Requests.Instructor;

/// <summary>
/// Request model for listing exam attempts/submissions with search and pagination criteria.
/// </summary>
public class ListExamSubmissionsRequest : BasePaginatedRequest
{
    /// <summary>
    /// Gets or sets the search filter for the student's name.
    /// </summary>
    public string? StudentName { get; set; }

    /// <summary>
    /// Gets or sets the filter for the exam attempt status.
    /// </summary>
    public ExamAttemptStatus? Status { get; set; }
}
