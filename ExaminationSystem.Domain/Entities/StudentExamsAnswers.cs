namespace ExaminationSystem.Domain.Entities;

public class StudentExamsAnswers : BaseModel
{
    public int StudentID { get; set; }
    public required Student Student { get; set; }

    public int ExamID { get; set; }
    public required Exam Exam { get; set; }

    public int QuestionID { get; set; }
    public required Question Question { get; set; }

    public int ChoiceID { get; set; }
    public required Choice Choice { get; set; }
}