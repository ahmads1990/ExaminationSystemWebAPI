using ExaminationSystem.Application.DTOs.Questions;

namespace ExaminationSystem.Application.DTOs.Exams;

/// <summary>
/// Data transfer object for exam details.
/// </summary>
public class ExamDto
{
    /// <summary>
    /// The type of exam (e.g., Quiz, Midterm, Final).
    /// </summary>
    public ExamType ExamType { get; set; }

    /// <summary>
    /// The title of the exam.
    /// </summary>
    public string Title { get; set; } = string.Empty;

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
    /// The current status of the exam (e.g., Draft, Published, Archived).
    /// </summary>
    public ExamStatus ExamStatus { get; set; }

    /// <summary>
    /// The deadline date for taking the exam.
    /// </summary>
    public DateTime DeadlineDate { get; set; }

    /// <summary>
    /// The unique identifier of the course this exam belongs to.
    /// </summary>
    public int CourseID { get; set; }

    /// <summary>
    /// The name of the course this exam belongs to.
    /// </summary>
    public string Course { get; set; }

    /// <summary>
    /// The list of questions assigned to this exam.
    /// </summary>
    public List<QuestionDto> Questions { get; set; } = new List<QuestionDto>();
}
