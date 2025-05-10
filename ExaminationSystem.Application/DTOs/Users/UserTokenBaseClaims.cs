namespace ExaminationSystem.Application.DTOs.Users;

public record UserTokenBaseClaims(int Uid, string Name, string Email)
{
    /// <summary>
    /// Validates that all claims are not null or empty.
    /// </summary>
    /// <returns>True if any claim is invalid, otherwise false.</returns>
    public bool AreClaimsInValid()
    {
        return Uid <= 0 ||
               string.IsNullOrEmpty(Name) ||
               string.IsNullOrEmpty(Email);
    }
}
