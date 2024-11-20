using ExaminationSystemWebAPI.Models;
using ExaminationSystemWebAPI.ViewModels.Choice;
using System.ComponentModel.DataAnnotations;

namespace ExaminationSystemWebAPI.ViewModels.Questions;

public class AddQuestionViewModel
{
    [Range(0, 3)]
    public QuestionLevel QuestionLevel { get; set; }
    [Length(5, 100)]
    public string TextBody { get; set; } = string.Empty;
    [Range(0f, 50f)]
    public float Score { get; set; }
    public ICollection<AddChoiceViewModel> Choices { get; set; } = default!;
}
