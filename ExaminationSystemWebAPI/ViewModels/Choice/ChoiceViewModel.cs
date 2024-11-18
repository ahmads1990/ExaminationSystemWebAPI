using ExaminationSystemWebAPI.Models;

namespace ExaminationSystemWebAPI.ViewModels.Choice;

public class ChoiceViewModel
{
    public string ID { get; set; } = string.Empty;
    public ChoiceOrder ChoiceOrder { get; set; }
    public string TextBody { get; set; } = string.Empty;
}
