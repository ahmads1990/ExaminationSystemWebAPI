using ExaminationSystemWebAPI.Helpers.Enums;

namespace ExaminationSystemWebAPI.Models;

public class Question : BaseModel
{
    public QuestionLevel QuestionLevel { get; set; }
    public string TextBody { get; set; } = string.Empty;
    public float Score { get; set; }
    public ChoiceOrder Answer { get; set; }
    public IEnumerable<Choice> Choices { get; set; } = default!;
    public ICollection<Exam> Exams { get; set; } = new List<Exam>();
}
