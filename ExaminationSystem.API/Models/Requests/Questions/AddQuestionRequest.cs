using ExaminationSystem.API.Models.Requests.Choices;

namespace ExaminationSystem.API.Models.Requests.Questions;

public class AddQuestionRequest
{
    public string Body { get; set; } = string.Empty;
    public int Score { get; set; }
    public byte QuestionLevel { get; set; }
    public byte AnswerOrder { get; set; }

    public IEnumerable<AddChoiceRequest> Choices { get; set; } = new List<AddChoiceRequest>();
}
