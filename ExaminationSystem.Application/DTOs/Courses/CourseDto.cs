namespace ExaminationSystem.Application.DTOs.Courses;

/// <summary>
/// Data transfer object for course details.
/// </summary>
public class CourseDto
{
    /// <summary>
    /// The unique identifier of the course.
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// The title of the course.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// A brief description of the course content.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Number of credit hours assigned to the course.
    /// </summary>
    public int CreditHours { get; set; }

    /// <summary>
    /// The unique identifier of the instructor teaching this course.
    /// </summary>
    public int InstructorID { get; set; }

    /// <summary>
    /// The name of the instructor teaching this course.
    /// </summary>
    public string InstructorName { get; set; } = string.Empty;

    /// <summary>
    /// The UTC date and time when the course was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }
}
