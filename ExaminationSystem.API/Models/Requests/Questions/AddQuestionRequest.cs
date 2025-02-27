using ExaminationSystem.API.Models.Requests.Choices;
using System.ComponentModel.DataAnnotations;

namespace ExaminationSystem.API.Models.Requests.Questions;

public class AddQuestionRequest
{
    [Length(5, 100)]
    public string Body { get; set; } = string.Empty;

    [Range(0, 50)]
    public int Score { get; set; }

    [Range(0, 3)]
    public byte QuestionLevel { get; set; }

    [Range(0, 4)]
    public byte AnswerOrder { get; set; }
    public IEnumerable<AddChoiceRequest> Choices { get; set; } = new List<AddChoiceRequest>();
}
