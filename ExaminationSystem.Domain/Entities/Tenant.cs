namespace ExaminationSystem.Domain.Entities;

/// <summary>
/// Represents a tenant (organization/institution) in the multi-tenant system.
/// This is a standalone lookup entity and does not inherit from BaseModel
/// to avoid circular TenantId references.
/// </summary>
public class Tenant
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
