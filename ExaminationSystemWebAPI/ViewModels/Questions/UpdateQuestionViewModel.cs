using ExaminationSystemWebAPI.Models;
using ExaminationSystemWebAPI.ViewModels.Choice;

namespace ExaminationSystemWebAPI.ViewModels.Questions;

public class UpdateQuestionViewModel
{
    public string ID { get; set; } = string.Empty;
    public QuestionLevel QuestionLevel { get; set; }
    public string TextBody { get; set; } = string.Empty;
    public float Score { get; set; }
    public IEnumerable<UpdateChoiceBodyViewModel> Choices { get; set; } = default!;
}
