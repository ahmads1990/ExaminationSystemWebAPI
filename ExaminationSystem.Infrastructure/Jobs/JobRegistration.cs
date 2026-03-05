using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace ExaminationSystem.Infrastructure.Jobs;

public static class JobRegistration
{
    public static void RegisterJobs(this IServiceCollection services)
    {
        services.AddScoped<SendEmailJob>();
        services.AddScoped<ICloseExamAttemptJob, CloseExamAttemptJob>();
    }
}