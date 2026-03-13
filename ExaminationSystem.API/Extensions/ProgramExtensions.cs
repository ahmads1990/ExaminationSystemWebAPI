using ExaminationSystem.API.Common.Configs;
using ExaminationSystem.API.Models.Responses;
using ExaminationSystem.Infrastructure.Data.Seeding;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
