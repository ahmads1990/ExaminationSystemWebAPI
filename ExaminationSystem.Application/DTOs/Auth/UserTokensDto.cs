namespace ExaminationSystem.Application.DTOs.Auth;

/// <summary>
/// Represents the JWT and refresh token pair returned after successful authentication.
/// </summary>
public class UserTokensDto
{
    /// <summary>
    /// The short-lived JWT access token used for authenticating API requests.
    /// </summary>
    public string JwtToken { get; set; } = string.Empty;

    /// <summary>
    /// The long-lived refresh token used to obtain new JWT tokens without re-authenticating.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// The user ID (populated on specific states like unverified email).
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// The tenant ID associated with the user account.
    /// </summary>
    public int? TenantId { get; set; }

    /// <summary>
    /// The tenant name associated with the user account.
    /// </summary>
    public string? TenantName { get; set; }
}
