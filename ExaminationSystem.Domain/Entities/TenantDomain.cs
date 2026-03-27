namespace ExaminationSystem.Domain.Entities;

/// <summary>
/// Maps a domain (hostname) to a specific tenant, enabling domain-based tenant resolution.
/// A tenant can have multiple domains; one should be marked as primary.
/// </summary>
public class TenantDomain
{
    public int ID { get; set; }
    public int TenantId { get; set; }
    public string Domain { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public Tenant Tenant { get; set; } = null!;
}
