using ExaminationSystemWebAPI.Models;

namespace ExaminationSystemWebAPI.ViewModels.Choice;

public class AddChoiceViewModel
{
    public ChoiceOrder ChoiceOrder { get; set; }
    public string TextBody { get; set; } = string.Empty;
}
