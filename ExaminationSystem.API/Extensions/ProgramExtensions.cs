using ExaminationSystem.API.Common.Configs;
using ExaminationSystem.API.Models.Responses;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;

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

                var response = new FailureResponse<object>(ErrorCode.ValidationError, combinedErrorMessage);

                return new BadRequestObjectResult(response);
            };
        });

        return services;
    }
}
