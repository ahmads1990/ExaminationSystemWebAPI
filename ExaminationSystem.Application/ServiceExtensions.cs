using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Application.Services;
using ExaminationSystem.Domain.Interfaces;
using ExaminationSystem.Infrastructure.Data.Repositories;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ExaminationSystem.Application;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IChoiceService, ChoiceService>();

        return services;
    }

    public static IServiceCollection AddMapsterConfiguration(this IServiceCollection services)
    {
        // Auto-scan for Mapster profiles
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly());

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>(); // Mapster Mapper

        return services;
    }
}
