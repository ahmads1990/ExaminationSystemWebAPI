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

        var host = context.Request.Host.Host.ToLowerInvariant();
        var tenantId = await tenantDomainResolver.ResolveTenantIdByDomainAsync(host, context.RequestAborted);

        if (tenantId.HasValue)
        {
            tenantAccessor.SetTenantId(tenantId.Value);
            await _next(context);
            return;
        }

        // Domain not found — apply configured action
        switch (_tenancyConfig.UnknownDomainAction)
        {
            case UnknownDomainAction.UseDefaultTenant:
                _logger.LogWarning("Unknown domain '{Domain}', falling back to default tenant {DefaultTenantId}", host, _tenancyConfig.DefaultTenantId);
                tenantAccessor.SetTenantId(_tenancyConfig.DefaultTenantId);
                await _next(context);
                return;

            case UnknownDomainAction.RejectRequest:
            default:
                _logger.LogWarning("Request rejected — no tenant mapped for domain '{Domain}'", host);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";

                var payload = new
                {
                    success = false,
                    data = (object?)null,
                    errorCode = 1011,
                    message = $"No tenant is configured for domain '{host}'"
                };

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                await context.Response.WriteAsJsonAsync(payload, options);
                return;
        }
    }
}
