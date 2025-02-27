using System.ComponentModel.DataAnnotations;

namespace ExaminationSystem.API.Models.Requests.Choices;

public class AddChoiceRequest
{
    [Length(5, 100)]
    public string Body { get; set; } = string.Empty;
}
