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
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
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
        services.Configure<JwtConfig>(configuration.GetSection("Jwt"));

        var systemOptions = configuration
            .GetSection(nameof(SystemServiceOptions))
            .Get<SystemServiceOptions>() ?? new SystemServiceOptions();

        services.AddDatabaseConfiguration(configuration, env);
        services.AddCacheConfiguration(configuration, systemOptions);

        services.AddSecurityConfiguration(configuration);
        services.AddHangfireConfiguration(configuration);

        return services;
    }

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

            o.Events = new JwtBearerEvents
            {
                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";

                    var isExamEndpoint = context.Request.Path.StartsWithSegments("/api/studentexams");
                    bool isExpired = context.AuthenticateFailure is SecurityTokenExpiredException;

                    int code = 1007; // Unauthorized
                    string msg = "You must be logged in to access this resource";

                    if (isExpired)
                    {
                        if (isExamEndpoint)
                        {
                            code = 1010; // ExamTimeout
                            msg = "Your exam time has expired";
                        }
                        else
                        {
                            code = 1006; // ExpiredToken
                            msg = "Your session has expired. Please login again";
                        }
                    }
                    else if (context.AuthenticateFailure != null)
                    {
                        code = 1009; // InvalidToken
                        msg = "Invalid token";
                    }

                    var payload = new
                    {
                        success = false,
                        data = (object?)null,
                        errorCode = code,
                        message = msg
                    };

                    var options = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                    };
                    await context.Response.WriteAsJsonAsync(payload, options);
                },
                OnForbidden = async context =>
                {
                    context.Response.StatusCode = 403;
                    context.Response.ContentType = "application/json";
                    var payload = new
                    {
                        success = false,
                        data = (object?)null,
                        errorCode = 1008, // Forbidden
                        message = "You don't have permission to access this resource"
                    };
                    var options = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                    };
                    await context.Response.WriteAsJsonAsync(payload, options);
                }
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

