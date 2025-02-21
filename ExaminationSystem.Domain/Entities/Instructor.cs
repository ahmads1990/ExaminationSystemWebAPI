namespace ExaminationSystem.Domain.Entities;

public class Instructor : BaseModel
{
    public int AppUserId { get; set; }
    public AppUser AppUser { get; set; } = default!;
}
