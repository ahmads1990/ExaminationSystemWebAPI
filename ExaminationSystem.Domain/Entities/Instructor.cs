namespace ExaminationSystem.Domain.Entities;

public class Instructor : BaseModel
{
    public int AppUserId { get; set; }
    public required AppUser AppUser { get; set; }

    public string Bio { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
}
