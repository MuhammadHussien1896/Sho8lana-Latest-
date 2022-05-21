using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace exp.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger _logger;
        private readonly IConfiguration configuration;

        public EmailSender(IConfiguration configuration,
                           ILogger<EmailSender> logger)
        {
            this.configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            string sendGridApiKey = configuration["SendGridKey"];

            if (string.IsNullOrEmpty(sendGridApiKey))
            {
                throw new Exception("Null SendGridKey");
            }
            await Execute(sendGridApiKey, subject, message, toEmail);
        }

        public async Task Execute(string apiKey, string subject, string message, string toEmail)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("sho8lanaservices@gmail.com", "Email Confirmation"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(toEmail));

            // Disable click tracking.
            // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
            msg.SetClickTracking(false, false);
            var response = await client.SendEmailAsync(msg);
            _logger.LogInformation(response.IsSuccessStatusCode
                                   ? $"Email to {toEmail} queued successfully!"
                                   : $"Failure Email to {toEmail}");
        }
    }
}
