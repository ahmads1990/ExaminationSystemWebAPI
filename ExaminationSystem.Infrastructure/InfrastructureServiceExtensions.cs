using ExaminationSystem.Application.InfraInterfaces;
using ExaminationSystem.Domain.Common;
using ExaminationSystem.Domain.Interfaces;
using ExaminationSystem.Infrastructure.Configs;
using ExaminationSystem.Infrastructure.Data;
using ExaminationSystem.Infrastructure.Data.Repositories;
using ExaminationSystem.Infrastructure.Jobs;
using ExaminationSystem.Infrastructure.Services;
using ExaminationSystem.Infrastructure.Services.Auth;
using ExaminationSystem.Infrastructure.Services.Email;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;

namespace ExaminationSystem.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IPasswordHasher<string>, PasswordHasher<string>>();
        services.AddScoped<IPasswordHelper, PasswordHelper>();
        services.AddScoped<IEmailService, EmailService>();

        services.AddSingleton<ITokenHelper, TokenHelper>();
        services.AddSingleton<ICachingService, CachingService>();

        return services;
    }

    public static IServiceCollection AddInfraStructureConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SMTPConfig>(configuration.GetSection(nameof(SMTPConfig)));
        services.Configure<JwtConfig>(configuration.GetSection("Jwt"));

        services.AddDatabaseConfiguration(configuration);
        services.AddRedisCacheConfiguration(configuration);

        services.AddSecurityConfiguration(configuration);
        services.AddHangfireConfiguration(configuration);

        return services;
    }

    #region Database Configuration

    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(Constants.DBConnectionStringName);
        services.AddDbContext<AppDbContext>(options =>
        {
            options
                .UseSqlServer(connectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .LogTo(log => Debug.WriteLine(log), LogLevel.Information)
                .EnableSensitiveDataLogging();
        });
        return services;
    }

    public static IServiceCollection AddRedisCacheConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(nameof(RedisConfig)).Get<RedisConfig>();
        services.AddStackExchangeRedisCache(options =>
        {
            options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions
            {
                EndPoints = { { settings!.Host, settings.Port } },
                User = settings.User,
                Password = settings.Password,
                Ssl = settings.Ssl,
                AbortOnConnectFail = settings.AbortOnConnectFail
            };
            options.InstanceName = settings.InstanceName;
        });
        return services;
    }

    #endregion

    #region Security Configuration

    public static IServiceCollection AddSecurityConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtConfig = configuration.GetSection("Jwt").Get<JwtConfig>()
            ?? throw new InvalidOperationException("Missing required configuration section: 'Jwt'.");

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
                ValidIssuer = jwtConfig.Issuer,
                ValidAudience = jwtConfig.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtConfig.Key)),
            };
        });

        return services;
    }

    #endregion

    #region Hangfire Configuration

    public static IServiceCollection AddHangfireConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(Constants.DBConnectionStringName);
        // Add Hangfire services
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(connectionString));

        services.AddHangfireServer();
        services.RegisterJobs();

        return services;
    }

    #endregion
}

