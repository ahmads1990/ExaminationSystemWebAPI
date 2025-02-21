namespace ExaminationSystem.Domain.Entities;

public class StudentExamsAnswers : BaseModel
{
    public int StudentID { get; set; }
    public Student Student { get; set; } = default!;

    public int ExamID { get; set; }
    public Exam Exam { get; set; } = default!;

    public int QuestionID { get; set; }
    public Question Question { get; set; } = default!;

    public int ChoiceID { get; set; }
    public Choice Choice { get; set; } = default!;
}