namespace ExaminationSystem.Domain.Entities;

public class StudentExamsAnswers : BaseModel
{
    public int StudentID { get; set; }
    public int QuestionID { get; set; }
    public int ChoiceID { get; set; }
    public int ExamAttemptID { get; set; }

    public Student Student { get; set; } = default!;
    public Question Question { get; set; } = default!;
    public Choice Choice { get; set; } = default!;
    public ExamAttempt ExamAttempt { get; set; } = default!;
}