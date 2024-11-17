using System.ComponentModel.DataAnnotations;

namespace ExaminationSystemWebAPI.Models;

public class Choice : BaseModel
{
    public ChoiceOrder ChoiceOrder { get; set; }
    public string TextBody { get; set; } = string.Empty;
}

public enum ChoiceOrder
{
    A = 0,
    B,
    C,
    D
}