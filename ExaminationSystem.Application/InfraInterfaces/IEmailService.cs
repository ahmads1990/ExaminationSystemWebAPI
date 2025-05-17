namespace ExaminationSystem.Application.InfraInterfaces;

public interface IEmailService
{
    public Task SendEmailAsync(string to, string subject, EmailTemplate template, Dictionary<string, string> templateModel);
}

