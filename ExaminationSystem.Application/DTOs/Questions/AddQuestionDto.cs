using ExaminationSystem.Application.DTOs.Choices;

namespace ExaminationSystem.Application.DTOs.Questions;

public class AddQuestionDto
{
    public string Body { get; set; } = string.Empty;
    public int Score { get; set; }
    public QuestionLevel QuestionLevel { get; set; }
    public ICollection<AddChoiceDto> Choices { get; set; } = new List<AddChoiceDto>();
}
