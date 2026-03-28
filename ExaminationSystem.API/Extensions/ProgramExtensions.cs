using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using ExaminationSystem.API.Common.Configs;
using ExaminationSystem.API.Models.Responses;
using ExaminationSystem.Domain.Common;
using ExaminationSystem.Infrastructure.Data.Seeding;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace ExaminationSystem.API.Extensions;

public static class ProgramExtensions
{
    public static IApplicationBuilder UseCustomCors(this IApplicationBuilder applicationBuilder, IConfiguration configuration)
    {
        var corsConfig = configuration.GetSection("Cors").Get<CorsConfig>();

        applicationBuilder.UseCors(builder =>
        {
            builder.WithOrigins(corsConfig?.AllowedOrigins ?? [])
                   .WithMethods(corsConfig?.AllowedMethods ?? [])
                   .WithHeaders(corsConfig?.AllowedHeaders ?? []);

        });

        return applicationBuilder;
    }

    public static IServiceCollection AddApiVersioningConfiguration(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    public static IServiceCollection AddRateLimitingConfiguration(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            
            options.AddSlidingWindowLimiter("sliding", limiterOptions =>
            {
                limiterOptions.PermitLimit = 100;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.SegmentsPerWindow = 6;
                limiterOptions.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });
        });

        return services;
    }

    public static IServiceCollection AddFluentValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(Program).Assembly);

        services.AddFluentValidationAutoValidation();

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errorMessages = context.ModelState
                        .Where(e => e.Value?.Errors.Count > 0)
                        .SelectMany(e => e.Value!.Errors)
                        .Select(er => er.ErrorMessage)
                        .ToList();

                var combinedErrorMessage = string.Join(" | ", errorMessages);

                var response = new ErrorResponse<object>(ApiErrorCode.ValidationFailed, combinedErrorMessage);

                return new BadRequestObjectResult(response);
            };
        });

        return services;
    }

    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Examination System API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter JWT token strictly."
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Set the comments path for the Swagger JSON and UI.
            var apiXmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var apiXmlPath = Path.Combine(AppContext.BaseDirectory, apiXmlFile);
            if (File.Exists(apiXmlPath))
                c.IncludeXmlComments(apiXmlPath);

            var appXmlFile = "ExaminationSystem.Application.xml";
            var appXmlPath = Path.Combine(AppContext.BaseDirectory, appXmlFile);
            if (File.Exists(appXmlPath))
                c.IncludeXmlComments(appXmlPath);
        });

        return services;
    }

    #region Security Configuration

    /// <summary>
    /// Configures JWT Bearer challenge events to return consistent API error responses.
    /// The core JWT authentication scheme is registered in the Infrastructure layer.
    /// </summary>
    public static IServiceCollection AddJwtBearerEvents(this IServiceCollection services)
    {
        services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.Events = new JwtBearerEvents
            {
                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";

                    var isExamEndpoint = context.Request.Path.StartsWithSegments(Constants.StudentExamsRoutePrefix);
                    bool isExpired = context.AuthenticateFailure is SecurityTokenExpiredException;

                    var errorCode = ApiErrorCode.Unauthorized;

                    if (isExpired)
                    {
                        errorCode = isExamEndpoint
                            ? ApiErrorCode.ExamTimeout
                            : ApiErrorCode.ExpiredToken;
                    }
                    else if (context.AuthenticateFailure != null)
                    {
                        errorCode = ApiErrorCode.InvalidToken;
                    }

                    var payload = new
                    {
                        success = false,
                        data = (object?)null,
                        errorCode = (int)errorCode,
                        message = errorCode.GetErrorMessage()
                    };

                    var jsonOptions = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                    };
                    await context.Response.WriteAsJsonAsync(payload, jsonOptions);
                },
                OnForbidden = async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    var payload = new
                    {
                        success = false,
                        data = (object?)null,
                        errorCode = (int)ApiErrorCode.Forbidden,
                        message = ApiErrorCode.Forbidden.GetErrorMessage()
                    };
                    var jsonOptions = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                    };
                    await context.Response.WriteAsJsonAsync(payload, jsonOptions);
                }
            };
        });

        return services;
    }

    #endregion

    public static async Task ApplyDatabaseMigrationsAndSeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ExaminationSystem.Infrastructure.Data.AppDbContext>();
            await dbContext.Database.MigrateAsync();
            
            await AppDbSeeder.SeedAsync(scope.ServiceProvider);
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating or seeding the database.");
            throw;
        }
    }
}
