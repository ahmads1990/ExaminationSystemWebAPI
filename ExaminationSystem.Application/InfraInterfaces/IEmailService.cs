namespace ExaminationSystem.Application.InfraInterfaces
{
    /// <summary>
    /// Defines the contract for sending emails using pre-defined templates.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email using a specific template.
        /// </summary>
        /// <param name="toName">Recipient name to display.</param>
        /// <param name="toEmail">Recipient email address.</param>
        /// <param name="subject">Email subject line.</param>
        /// <param name="template">Template type (HTML/Text).</param>
        /// <param name="templateModel">Dictionary of placeholders and values.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        Task SendEmailAsync(
            string toName, string toEmail, string subject, EmailTemplate template,
            Dictionary<string, string> templateModel, CancellationToken cancellationToken = default);
    }
}
