using ExaminationSystem.API.Common.Configs;

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
}
