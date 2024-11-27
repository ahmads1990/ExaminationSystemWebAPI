using ExaminationSystemWebAPI.Helpers.Enums;

namespace ExaminationSystemWebAPI.Models;

public class Choice : BaseModel
{
    public ChoiceOrder ChoiceOrder { get; set; }
    public string TextBody { get; set; } = string.Empty;
    public Question Question { get; set; } = default!;
}
