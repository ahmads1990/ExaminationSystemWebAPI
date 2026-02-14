namespace ExaminationSystem.Domain.Entities;

public class RefreshToken : BaseModel
{
    public string TokenHash { get; set; } = string.Empty;
    public bool IsRevoked { get; set; }
    public DateTime ExpirationDate { get; set; }

    public int UserId { get; set; }
    public AppUser User { get; set; } = default!;
}
