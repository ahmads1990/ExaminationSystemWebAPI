using ExaminationSystem.Application.InfraInterfaces;
using ExaminationSystem.Domain.Common;
using ExaminationSystem.Domain.Interfaces;
using ExaminationSystem.Infrastructure.Configs;
using ExaminationSystem.Infrastructure.Data;
using ExaminationSystem.Infrastructure.Data.Repositories;
using ExaminationSystem.Infrastructure.Jobs;
using ExaminationSystem.Infrastructure.Services;
using ExaminationSystem.Infrastructure.Services.Auth;
using ExaminationSystem.Infrastructure.Services.Cache;
using ExaminationSystem.Infrastructure.Services.Email;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
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

        return services;
    }

    public static IServiceCollection AddInfraStructureConfiguration(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        services.Configure<SMTPConfig>(configuration.GetSection(nameof(SMTPConfig)));
        services.Configure<JwtConfig>(configuration.GetSection(Constants.JwtConfigSectionName));

        var systemOptions = configuration
            .GetSection(nameof(SystemServiceOptions))
            .Get<SystemServiceOptions>() ?? new SystemServiceOptions();

        services.AddDatabaseConfiguration(configuration, env);
        services.AddCacheConfiguration(configuration, systemOptions);
        services.AddHangfireConfiguration(configuration);
        services.AddJwtAuthentication(configuration);

        return services;
    }

    #region JWT Authentication

    /// <summary>
    /// Registers the core JWT Bearer authentication scheme with token validation parameters.
    /// HTTP-specific challenge/forbidden events should be configured in the API layer.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtConfig = configuration.GetSection(Constants.JwtConfigSectionName).Get<JwtConfig>()
            ?? throw new InvalidOperationException($"Missing required configuration section: '{Constants.JwtConfigSectionName}'.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = false;
            options.TokenValidationParameters = new TokenValidationParameters
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

    #region Database Configuration

    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        var connectionString = configuration.GetConnectionString(Constants.DBConnectionStringName);
        services.AddDbContext<AppDbContext>(options =>
        {
            options
                .UseSqlServer(connectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .LogTo(log => Debug.WriteLine(log), LogLevel.Information);

            if (env.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
            }
        });
        return services;
    }

    public static IServiceCollection AddCacheConfiguration(this IServiceCollection services, IConfiguration configuration, SystemServiceOptions systemOptions)
    {
        if (systemOptions.UseMemoryCache)
        {
            // In-process memory cache — no Redis required
            services.AddMemoryCache();
            services.AddSingleton<ICachingService, MemoryCachingService>();
        }
        else
        {
            // Distributed Redis cache
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
            services.AddSingleton<ICachingService, CachingService>();
        }

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
    
    #region Serilog Configuration

    public static void AddSerilogConfiguration(IConfiguration configuration)
    {
        var systemOptions = configuration
            .GetSection(nameof(SystemServiceOptions))
            .Get<SystemServiceOptions>() ?? new SystemServiceOptions();

        // Build Serilog: enrichers and levels come from appsettings; sinks are added in code
        // so WriteToSeq=false will never even attempt a Seq connection.
        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} | {Message:lj}{NewLine}{Exception}"
            );

        if (systemOptions.WriteToSeq)
        {
            var seqUrl = configuration["SeqUrl"] ?? "http://localhost:5341";
            loggerConfig = loggerConfig.WriteTo.Seq(seqUrl);
        }
        else
        {
            loggerConfig = loggerConfig.WriteTo.File(
                path: "logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} | {Message:lj}{NewLine}{Exception}"
            );
        }

        Log.Logger = loggerConfig.CreateLogger();
    }

    #endregion
}

