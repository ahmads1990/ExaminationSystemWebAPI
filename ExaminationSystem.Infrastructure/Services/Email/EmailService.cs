using ExaminationSystem.Application.InfraInterfaces;
using ExaminationSystem.Infrastructure.Configs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Text.RegularExpressions;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace ExaminationSystem.Infrastructure.Services.Email
{
    /// <summary>
    /// Provides functionality for sending emails using SMTP with support for HTML and plain text templates.
    /// </summary>
    public class EmailService : IEmailService
    {
        #region Fields

        private const string TemplatesFolder = "Templates/EmailTemplates";
        private readonly SMTPConfig _smtpConfig;
        private readonly string _templateRoot;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailService"/> class.
        /// </summary>
        /// <param name="sMTPConfig">The SMTP configuration settings.</param>
        /// <param name="env">The web hosting environment for locating template files.</param>
        public EmailService(IOptions<SMTPConfig> sMTPConfig, IWebHostEnvironment env)
        {
            _smtpConfig = sMTPConfig.Value;
            _templateRoot = Path.Combine(env.ContentRootPath, TemplatesFolder);
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public async Task SendEmailAsync(
            string toName, string toEmail, string subject, EmailTemplate template,
            Dictionary<string, string> templateModel, CancellationToken cancellationToken = default)
        {
            ValidateEmailParameters(toEmail, subject);

            var message = await CreateEmailMessage(toName, toEmail, subject, template, templateModel);

            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(_smtpConfig.Host, _smtpConfig.Port, _smtpConfig.EnableSsl, cancellationToken);
                    await client.AuthenticateAsync(_smtpConfig.Username, _smtpConfig.Password, cancellationToken);
                    await client.SendAsync(message, cancellationToken);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to send email.", ex);
                }
                finally
                {
                    if (client.IsConnected)
                        await client.DisconnectAsync(true, cancellationToken);
                }
            }
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Validates that essential email parameters are provided.
        /// </summary>
        /// <param name="toEmail">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <exception cref="ArgumentException">Thrown when email or subject is missing.</exception>
        private void ValidateEmailParameters(string toEmail, string subject)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("Recipient email is required.", nameof(toEmail));

            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("Email subject is required.", nameof(subject));
        }

        /// <summary>
        /// Creates a new email message based on the provided parameters and template.
        /// </summary>
        private async Task<MimeMessage> CreateEmailMessage(
            string toName, string toEmail, string subject, EmailTemplate template,
            Dictionary<string, string> templateModel)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpConfig.FromName, _smtpConfig.FromEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;
            message.Body = await BuildEmailBody(template, templateModel);

            return message;
        }

        /// <summary>
        /// Builds the email body from the specified HTML and text templates.
        /// </summary>
        private async Task<MimeEntity> BuildEmailBody(EmailTemplate template, Dictionary<string, string> templateModel)
        {
            var htmlEmailBody = await FetchEmailTemplate(template, templateModel);
            var txtEmailBody = await FetchEmailTemplate(template, templateModel, false);

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlEmailBody,
                TextBody = txtEmailBody
            };

            return bodyBuilder.ToMessageBody();
        }

        /// <summary>
        /// Loads and replaces placeholders within an email template file.
        /// </summary>
        /// <param name="template">The template identifier.</param>
        /// <param name="templateModel">The placeholder values to inject into the template.</param>
        /// <param name="isHtml">Determines whether to load the HTML or text version of the template.</param>
        private async Task<string> FetchEmailTemplate(EmailTemplate template, Dictionary<string, string> templateModel, bool isHtml = true)
        {
            var templateName = template.ToString();
            var templateFileType = isHtml ? "html" : "txt";
            var templatePath = Path.Combine(_templateRoot, templateName, $"{templateName}.{templateFileType}");

            if (!File.Exists(templatePath))
                throw new FileNotFoundException($"Email template not found: {templatePath}");

            var body = await File.ReadAllTextAsync(templatePath);

            if (templateModel != null)
            {
                foreach (var (key, value) in templateModel)
                {
                    body = Regex.Replace(body, $"{{{{{key}}}}}", value, RegexOptions.IgnoreCase);
                }
            }

            return body;
        }

        #endregion
    }
}
