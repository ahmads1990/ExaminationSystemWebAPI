using System.ComponentModel.DataAnnotations;

namespace ExaminationSystemWebAPI.ViewModels.Choice;

public class UpdateChoiceBodyViewModel
{
    public string ID { get; set; } = string.Empty;
    [Length(5, 100)]
    public string TextBody { get; set; } = string.Empty;
}
