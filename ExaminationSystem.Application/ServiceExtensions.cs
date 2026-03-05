using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Application.Services;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ExaminationSystem.Application;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IExamService, ExamService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IInstructorService, InstructorService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IStudentCourseService, StudentCourseService>();
        services.AddScoped<IStudentExamService, StudentExamService>();

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
