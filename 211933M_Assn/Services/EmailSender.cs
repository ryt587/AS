using Microsoft.Extensions.Options;
using SendGrid.Helpers.Mail;
using SendGrid;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace _211933M_Assn.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;

        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor,
                           ILogger<EmailSender> logger, IConfiguration _config)
        {
            Options = optionsAccessor.Value;
            _logger = logger;
            this._config = _config;
        }

        public AuthMessageSenderOptions Options { get; } //Set with Secret Manager.

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            await Execute(subject, message, toEmail);
        }

        public async Task Execute(string subject, string message, string toEmail)
        {
            var client = new SendGridClient(_config["SendGrid:Key"]);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("nigelngyingxuan@gmail.com", "App Security"),
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
                                   : $"{response.StatusCode.ToString()}");
        }
    }
}
