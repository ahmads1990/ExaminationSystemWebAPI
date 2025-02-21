namespace ExaminationSystem.Domain.Entities;

public class ExamQuestion : BaseModel
{
    public int ExamId { get; set; }
    public Exam Exam { get; set; } = default!;

    public int QuestionId { get; set; }
    public Question Question { get; set; } = default!;
}
