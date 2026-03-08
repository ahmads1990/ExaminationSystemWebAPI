namespace ExaminationSystem.API.Models.Requests.Exams;

/// <summary>
/// Model for adding a new exam.
/// </summary>
public class AddExamRequest
{
    /// <summary>
    /// The type of exam (e.g., Quiz, Midterm, Final).
    /// </summary>
    public ExamType ExamType { get; set; }

    /// <summary>
    /// Maximum duration allowed for the exam in minutes.
    /// </summary>
    public int MaxDurationInMinutes { get; set; }

    /// <summary>
    /// Total grade points for the exam.
    /// </summary>
    public int TotalGrade { get; set; }

    /// <summary>
    /// Minimum score required to pass the exam.
    /// </summary>
    public decimal PassingScore { get; set; }

    /// <summary>
    /// Maximum number of attempts allowed for a student.
    /// </summary>
    public int MaxAttempts { get; set; }

    /// <summary>
    /// Whether to shuffle the order of questions for each student.
    /// </summary>
    public bool ShuffleQuestions { get; set; }

    /// <summary>
    /// Whether to show the results to the student immediately after submission.
    /// </summary>
    public bool ShowResultsImmediately { get; set; }

    /// <summary>
    /// The deadline date for taking the exam.
    /// </summary>
    public DateTime DeadlineDate { get; set; }

    /// <summary>
    /// The unique identifier of the course this exam belongs to.
    /// </summary>
    public int CourseID { get; set; }
}
