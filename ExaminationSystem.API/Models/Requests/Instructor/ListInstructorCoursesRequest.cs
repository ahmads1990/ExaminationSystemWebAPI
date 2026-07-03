namespace ExaminationSystem.API.Models.Requests.Instructor;

/// <summary>
/// Request model for listing instructor courses with search and pagination criteria.
/// </summary>
public class ListInstructorCoursesRequest : BasePaginatedRequest
{
    /// <summary>
    /// Gets or sets the search filter for the course name.
    /// </summary>
    public string? CourseName { get; set; }
}
