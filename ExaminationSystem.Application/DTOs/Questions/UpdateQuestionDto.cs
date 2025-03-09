using ExaminationSystem.Application.DTOs.Choices;

namespace ExaminationSystem.Application.DTOs.Questions;

public class UpdateQuestionDto : AddQuestionDto
{
    public int ID { get; set; }
    public new ICollection<UpdateChoiceDto> Choices { get; set; } = new List<UpdateChoiceDto>();
}