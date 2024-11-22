namespace ExaminationSystemWebAPI.Models.Joins;

public class ExamQuestions
{
    public string ID { get; set; } = Guid.NewGuid().ToString();
    public int Order { get; set; }

    public string ExamID { get; set; } = string.Empty;
    public Exam Exam { get; set; } = default!;

    public string QuestionID { get; set; } = string.Empty;
    public Question Question { get; set; } = default!;
}
