using ExaminationSystem.Application.InfraInterfaces;
using ExaminationSystem.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ExaminationSystem.Application.UseCases
{
    /// <summary>
    /// Represents a background job responsible for sending emails using the configured <see cref="IEmailService"/>.
    /// </summary>
    public class SendEmailJob : ISendEmailJob
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<SendEmailJob> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendEmailJob"/> class.
        /// </summary>
        /// <param name="emailService">The email service used to send emails.</param>
        /// <param name="logger">The logger.</param>
        public SendEmailJob(IEmailService emailService, ILogger<SendEmailJob> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Executes the email-sending job asynchronously.
        /// </summary>
        /// <param name="toName">The recipient's display name.</param>
        /// <param name="toEmail">The recipient's email address.</param>
        /// <param name="subject">The subject line of the email.</param>
        /// <param name="template">The email template to be used for the message body.</param>
        /// <param name="parameters">The dictionary of placeholder values to inject into the template.</param>
        /// <param name="tenantId">Optional tenant ID for job identification and debugging.</param>
        /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
        public async Task Execute(string toName, string toEmail, string subject, EmailTemplate template,
            Dictionary<string, string> parameters, int? tenantId = null, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Executing SendEmailJob for {ToEmail} with subject {Subject} (TenantId: {TenantId})", toEmail, subject, tenantId?.ToString() ?? "N/A");
            await _emailService.SendEmailAsync(toName, toEmail, subject, template, parameters, cancellationToken);
            _logger.LogInformation("SendEmailJob finished successfully for {ToEmail} (TenantId: {TenantId})", toEmail, tenantId?.ToString() ?? "N/A");
        }
    }
}
