using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MiniStore.Helpers;
using MiniStore.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace MiniStore.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                var smtpHost = string.IsNullOrWhiteSpace(_settings.Host)
                    ? _settings.SmtpServer
                    : _settings.Host;

                await client.ConnectAsync(smtpHost, _settings.Port, SecureSocketOptions.StartTls);
                if (!string.IsNullOrEmpty(_settings.Username))
                {
                    await client.AuthenticateAsync(_settings.Username, _settings.Password);
                }
                await client.SendAsync(message);
            }
            finally
            {
                await client.DisconnectAsync(true);
                client.Dispose();
            }
        }
    }
}
