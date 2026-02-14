using ExaminationSystem.Application.DTOs.Choices;

namespace ExaminationSystem.Application.DTOs.Questions;

public class UpdateQuestionDto
{
    public int ID { get; set; }
    public string Body { get; set; } = string.Empty;
    public int Score { get; set; }
    public QuestionLevel QuestionLevel { get; set; }
    public ICollection<UpdateChoiceDto> Choices { get; set; } = new List<UpdateChoiceDto>();
}