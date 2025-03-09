using ExaminationSystem.Application.DTOs.Choices;

namespace ExaminationSystem.API.Models.Requests.Questions;

public class UpdateQuestionRequest : AddQuestionRequest
{
    public int ID { get; set; }
    public new ICollection<UpdateChoiceDto> Choices { get; set; } = new List<UpdateChoiceDto>();
}
