using ExaminationSystem.Application.Common;
using ExaminationSystem.Application.InfraInterfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ExaminationSystem.API.Middlewares;

/// <summary>
/// Intercepts requests to check if the provided JWT Token JTI exists in the Redis Blacklist.
/// If it exists, returns a 401 Unauthorized, effectively killing the pipeline.
/// </summary>
public class TokenBlacklistMiddleware
{
    private readonly RequestDelegate _next;

    public TokenBlacklistMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var jti = context.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (!string.IsNullOrEmpty(jti))
            {
                var cachingService = context.RequestServices.GetRequiredService<ICachingService>();
                var cacheKey = $"blacklist:jti:{jti}";

                // Extract tenantId from claims for tenant-aware cache key lookup
                int? tenantId = null;
                var tenantClaim = context.User.FindFirst(CustomClaimTypes.TenantId)?.Value;
                if (int.TryParse(tenantClaim, out var parsedTenantId))
                    tenantId = parsedTenantId;

                var isBlacklisted = await cachingService.GetAsync(cacheKey, tenantId);

                if (!string.IsNullOrEmpty(isBlacklisted))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\": \"Token has been revoked or logged out.\"}");
                    return;
                }
            }
        }

        await _next(context);
    }
}
