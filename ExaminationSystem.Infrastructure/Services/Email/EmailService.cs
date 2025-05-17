using ExaminationSystem.Application.InfraInterfaces;
using ExaminationSystem.Infrastructure.Configs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace ExaminationSystem.Infrastructure.Services.Email;

public class EmailService : IEmailService
{
    private readonly SMTPConfig _smtpConfig;
    private readonly string _templateRoot;

    public EmailService(IOptions<SMTPConfig> sMTPConfig, IWebHostEnvironment env)
    {
        _smtpConfig = sMTPConfig.Value;
        _templateRoot = Path.Combine(env.ContentRootPath, "Templates");
    }

    public async Task SendEmailAsync(string to, string subject, EmailTemplate template, Dictionary<string, string> templateModel)
    {
        var emailBody = await BuildEmailBody(template, templateModel);

        using var smtpClient = new SmtpClient(_smtpConfig.Host, _smtpConfig.Port)
        {
            Credentials = new NetworkCredential(_smtpConfig.Username, _smtpConfig.Password),
            EnableSsl = _smtpConfig.EnableSsl
        };

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(_smtpConfig.FromEmail, _smtpConfig.FromName),
            To = { to },
            Subject = subject,
            Body = emailBody,
            IsBodyHtml = _smtpConfig.IsBodyHtml
        };

        try
        {
            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to send email.", ex);
        }
    }

    private async Task<string> BuildEmailBody(EmailTemplate template, Dictionary<string, string> templateModel)
    {
        var templatePath = $"{_templateRoot}/EmailTemplates/{template.ToString()}.html";

        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"Email template '{template}' not found at path: {templatePath}");

        var body = await File.ReadAllTextAsync(templatePath);

        foreach (var (key, value) in templateModel)
        {
            body = body.Replace($"{{{key}}}", value);
        }

        return body;
    }
}

