namespace ExaminationSystem.Domain.Entities;

public class ExamQuestion : BaseModel
{
    public int ExamId { get; set; }
    public required Exam Exam { get; set; }

    public int QuestionId { get; set; }
    public required Question Question { get; set; }
}
