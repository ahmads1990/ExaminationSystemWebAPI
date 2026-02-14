using ExaminationSystem.API.Models.Requests.Choices;

namespace ExaminationSystem.API.Models.Requests.Questions;

public class UpdateQuestionRequest
{
    public int ID { get; set; }
    public string Body { get; set; } = string.Empty;
    public int Score { get; set; }
    public QuestionLevel QuestionLevel { get; set; }

    public ICollection<UpdateChoiceRequest> Choices { get; set; } = new List<UpdateChoiceRequest>();
}
