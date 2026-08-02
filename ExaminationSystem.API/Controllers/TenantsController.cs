using ExaminationSystem.Application.DTOs.Tenants;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.API.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystem.API.Controllers;

/// <summary>
/// Provides tenant lookup operations for frontend dropdowns.
/// </summary>
public class TenantsController : BaseController
{
    private readonly ITenantService _tenantService;

    public TenantsController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    /// <summary>
    /// Gets all active tenants for dropdown lists (cached).
    /// </summary>
    /// <returns>A list of active tenant names and IDs.</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SuccessResponse<List<TenantLookupDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var tenants = await _tenantService.GetAllTenantsAsync(cancellationToken);
        return Ok(new SuccessResponse<List<TenantLookupDto>>(tenants));
    }

    /// <summary>
    /// Gets currently resolved tenant context.
    /// </summary>
    [HttpGet("current")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SuccessResponse<TenantLookupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrent([FromServices] ITenantAccessor tenantAccessor, CancellationToken cancellationToken)
    {
        int currentTenantId = tenantAccessor.TenantId ?? 1;
        var tenant = await _tenantService.GetTenantByIdAsync(currentTenantId, cancellationToken);
        return Ok(new SuccessResponse<TenantLookupDto>(tenant ?? new TenantLookupDto { ID = currentTenantId, Name = "Default University" }));
    }

    /// <summary>
    /// Gets a tenant by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SuccessResponse<TenantLookupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var tenant = await _tenantService.GetTenantByIdAsync(id, cancellationToken);
        if (tenant == null)
            return NotFound(new ErrorResponse<object>(ApiErrorCode.ResourceNotFound));

        return Ok(new SuccessResponse<TenantLookupDto>(tenant));
    }
}
