namespace ExaminationSystem.Application.DTOs;

/// <summary>
/// Represents an entity that was rejected during a bulk operation, along with the reason.
/// </summary>
public class RejectedEntityDto
{
    public int Id { get; set; }
    public RejectionReason Reason { get; set; }
}
