namespace ExaminationSystem.Domain.Entities;

public class ExamAttempt : BaseModel
{
    public int StudentId { get; set; }
    public int ExamId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public ExamAttemptStatus ExamAttemptStatus { get; set; } = ExamAttemptStatus.NotStarted;
    public double? Score { get; set; }

    public Exam Exam { get; set; } = default!;
    public Student Student { get; set; } = default!;
    public ICollection<StudentExamsAnswers> Answers { get; set; } = new List<StudentExamsAnswers>();
}
