using ExaminationSystem.Application.InfraInterfaces;
using ExaminationSystem.Domain.Interfaces;
using ExaminationSystem.Infrastructure.Configs;
using ExaminationSystem.Infrastructure.Data.Repositories;
using ExaminationSystem.Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ExaminationSystem.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IPasswordHasher<string>, PasswordHasher<string>>();
        services.AddScoped<IPasswordHelper, PasswordHelper>();
        services.AddSingleton<ITokenHelper, TokenHelper>();

        return services;
    }

    public static IServiceCollection AddSecurityConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure jwt helper class to use jwt config info
        SetJwtConfig(configuration);

        // Add Authentication with jwt config
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(o =>
        {
            o.RequireHttpsMetadata = false;
            o.SaveToken = false;
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidIssuer = JwtConfig.Issuer,
                ValidAudience = JwtConfig.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(JwtConfig.Key)),
            };
        });

        return services;
    }

    private static void SetJwtConfig(IConfiguration configuration)
    {
        JwtConfig.Key = configuration.GetSection("Jwt:Key")?.Value ?? string.Empty;
        JwtConfig.Issuer = configuration.GetSection("Jwt:Issuer")?.Value ?? string.Empty;
        JwtConfig.Audience = configuration.GetSection("Jwt:Audience")?.Value ?? string.Empty;
        JwtConfig.DurationInHours = int.Parse(configuration.GetSection("Jwt:DurationInHours")?.Value ?? "0");
    }
}

