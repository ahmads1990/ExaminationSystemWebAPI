using ExaminationSystem.Application.InfraInterfaces;

namespace ExaminationSystem.Application.UseCases;
public class SendEmailJob
{
    private readonly IEmailService _emailService;

    public SendEmailJob(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Execute(string recipient, string subject, EmailTemplate template, Dictionary<string, string> parameters)
    {
        await _emailService.SendEmailAsync(recipient, subject, template, parameters);
    }
}
