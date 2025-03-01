using ExaminationSystem.Application.DTOs.Choices;

namespace ExaminationSystem.Application.DTOs.Questions;

public class QuestionDto
{
    public int ID { get; set; }
    public string Body { get; set; } = string.Empty;
    public int Score { get; set; }
    public QuestionLevel QuestionLevel { get; set; }

    public int AnswerId { get; set; }
    public IEnumerable<ChoiceDto> Choices { get; set; } = new List<ChoiceDto>();
}
