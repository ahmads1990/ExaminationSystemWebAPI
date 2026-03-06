namespace ExaminationSystem.Domain.Entities;

public class Exam : BaseModel
{
    public string Title { get; set; } = string.Empty;
    public ExamType ExamType { get; set; }
    public ExamStatus ExamStatus { get; set; } = ExamStatus.Draft;
    public DateTime DeadlineDate { get; set; }
    public DateTime? PublishDate { get; set; }

    // Settings
    public int MaxDurationInMinutes { get; set; }
    public int TotalGrade { get; set; }
    public decimal PassingScore { get; set; }
    public int MaxAttempts { get; set; } = 1;
    public bool ShuffleQuestions { get; set; }

    public int CourseID { get; set; }
    public Course Course { get; set; }
    public ICollection<ExamQuestion> ExamQuestions { get; set; } = new List<ExamQuestion>();
    public ICollection<ExamAttempt> ExamAttempts { get; set; } = new List<ExamAttempt>();
}
