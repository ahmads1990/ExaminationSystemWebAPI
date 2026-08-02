using ExaminationSystem.Application.Common;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Infrastructure.Configs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ExaminationSystem.API.Middlewares;

/// <summary>
/// Resolves the current tenant from the incoming request's domain (hostname).
/// Must be registered before authentication in the pipeline so tenant context
/// is available for all downstream middleware and services.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TenancyConfig _tenancyConfig;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(RequestDelegate next, IOptions<TenancyConfig> tenancyOptions, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _tenancyConfig = tenancyOptions.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantDomainResolver tenantDomainResolver, ITenantAccessor tenantAccessor)
    {
        var requestPath = context.Request.Path.Value ?? string.Empty;

        // Skip tenant resolution for excluded paths
        if (_tenancyConfig.ExcludedPaths.Any(path => requestPath.StartsWith(path, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        // 1. Try resolving tenant from X-Tenant-Id HTTP Header
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerVal) &&
            int.TryParse(headerVal.FirstOrDefault(), out var headerTenantId))
        {
            tenantAccessor.SetTenantId(headerTenantId);
            await _next(context);
            return;
        }

        // 2. Try resolving tenant from query string (?tenantId=2 or ?tenant=2)
        if ((context.Request.Query.TryGetValue("tenantId", out var queryVal) ||
             context.Request.Query.TryGetValue("tenant", out queryVal)) &&
            int.TryParse(queryVal.FirstOrDefault(), out var queryTenantId))
        {
            tenantAccessor.SetTenantId(queryTenantId);
            await _next(context);
            return;
        }

        // 3. Fallback to domain lookup
        var host = context.Request.Host.Host.ToLowerInvariant();
        var tenantId = await tenantDomainResolver.ResolveTenantIdByDomainAsync(host, context.RequestAborted);

        if (tenantId.HasValue)
        {
            tenantAccessor.SetTenantId(tenantId.Value);
            await _next(context);
            return;
        }

        // 4. Default tenant fallback
        _logger.LogInformation("Domain '{Domain}' not mapped, using default tenant {DefaultTenantId}", host, _tenancyConfig.DefaultTenantId);
        tenantAccessor.SetTenantId(_tenancyConfig.DefaultTenantId);
        await _next(context);
    }
}
