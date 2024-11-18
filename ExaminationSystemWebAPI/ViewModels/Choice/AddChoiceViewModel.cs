using ExaminationSystemWebAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace ExaminationSystemWebAPI.ViewModels.Choice;

public class AddChoiceViewModel
{
    [Range(0, 3)]
    public ChoiceOrder ChoiceOrder { get; set; }
    [Length(5,100)]
    public string TextBody { get; set; } = string.Empty;
}
