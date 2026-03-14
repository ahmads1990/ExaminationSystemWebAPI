namespace ExaminationSystem.Application.DTOs.Tenants;

/// <summary>
/// Lightweight DTO for tenant lookup dropdown lists.
/// </summary>
public class TenantLookupDto
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;
}
