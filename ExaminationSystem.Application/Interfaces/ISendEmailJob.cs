namespace ExaminationSystem.Application.Interfaces
{
    /// <summary>
    /// Defines the contract for background jobs responsible for sending emails.
    /// </summary>
    public interface ISendEmailJob
    {
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
        Task Execute(string toName, string toEmail, string subject, EmailTemplate template,
            Dictionary<string, string> parameters, int? tenantId = null, CancellationToken cancellationToken = default);
    }
}
