namespace ExaminationSystem.Domain.Entities;

public class Question : BaseModel
{
    public string Body { get; set; } = string.Empty;
    public int Score { get; set; }
    public QuestionLevel QuestionLevel { get; set; }

    public byte AnswerOrder { get; set; }
    public ICollection<Choice> Choices { get; set; } = new List<Choice>();
    public IEnumerable<ExamQuestion> ExamQuestions { get; set; } = new List<ExamQuestion>();
}
