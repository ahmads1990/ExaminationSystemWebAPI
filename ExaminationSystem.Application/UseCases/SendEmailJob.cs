using ExaminationSystem.Application.InfraInterfaces;
using ExaminationSystem.Application.Interfaces;

namespace ExaminationSystem.Application.UseCases
{
    /// <summary>
    /// Represents a background job responsible for sending emails using the configured <see cref="IEmailService"/>.
    /// </summary>
    public class SendEmailJob : ISendEmailJob
    {
        private readonly IEmailService _emailService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendEmailJob"/> class.
        /// </summary>
        /// <param name="emailService">The email service used to send emails.</param>
        public SendEmailJob(IEmailService emailService)
        {
            _emailService = emailService;
        }

        /// <summary>
        /// Executes the email-sending job asynchronously.
        /// </summary>
        /// <param name="toName">The recipient's display name.</param>
        /// <param name="toEmail">The recipient's email address.</param>
        /// <param name="subject">The subject line of the email.</param>
        /// <param name="template">The email template to be used for the message body.</param>
        /// <param name="parameters">The dictionary of placeholder values to inject into the template.</param>
        /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
        public async Task Execute(string toName, string toEmail, string subject, EmailTemplate template,
            Dictionary<string, string> parameters, CancellationToken cancellationToken = default)
        {
            await _emailService.SendEmailAsync(toName, toEmail, subject, template, parameters, cancellationToken);
        }
    }
}
