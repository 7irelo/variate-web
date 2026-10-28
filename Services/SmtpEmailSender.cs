using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace variate.Services
{
    public class SmtpOptions
    {
        public const string SectionName = "Smtp";

        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string From { get; set; } = string.Empty;

        public bool IsConfigured => !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(From);
    }

    // Sends Identity's account emails (confirmation, password reset) over SMTP with TLS.
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _options;

        public SmtpEmailSender(SmtpOptions options)
        {
            _options = options;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using var message = new MailMessage(_options.From, email, subject, htmlMessage) { IsBodyHtml = true };
            using var client = new SmtpClient(_options.Host, _options.Port) { EnableSsl = true };
            if (!string.IsNullOrEmpty(_options.Username))
            {
                client.Credentials = new NetworkCredential(_options.Username, _options.Password);
            }
            await client.SendMailAsync(message);
        }
    }
}
