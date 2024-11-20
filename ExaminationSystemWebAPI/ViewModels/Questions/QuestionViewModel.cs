using ExaminationSystemWebAPI.ViewModels.Choice;

namespace ExaminationSystemWebAPI.ViewModels.Questions;

public class QuestionViewModel
{
    public string ID { get; set; } = string.Empty;
    public string QuestionLevel { get; set; } = string.Empty;
    public string TextBody { get; set; } = string.Empty;
    public float Score { get; set; }
    public IEnumerable<ChoiceViewModel> Choices { get; set; } = default!;
}
