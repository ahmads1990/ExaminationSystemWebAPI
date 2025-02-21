namespace ExaminationSystem.Domain.Entities;

public class Choice : BaseModel
{
    public string Body { get; set; } = string.Empty;

    public int QuestionId { get; set; } 
    public Question Question { get; set; } = default!;
}
