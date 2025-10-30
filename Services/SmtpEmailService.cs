using ASH_Translation.Services.Interface;
using MailKit.Net.Smtp;
using MimeKit;

namespace ASH_Translation.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public SmtpEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlMessage)
        {
            var email = new MimeMessage();
            var from = Environment.GetEnvironmentVariable("EMAIL_FROM") ?? _config["Email:From"];
            email.From.Add(MailboxAddress.Parse(from));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = htmlMessage
            };

            using var smtp = new SmtpClient();
            try
            {
                var host = Environment.GetEnvironmentVariable("EMAIL_HOST") ?? _config["Email:Host"];
                var portStr = Environment.GetEnvironmentVariable("EMAIL_PORT") ?? _config["Email:Port"];
                int port = int.TryParse(portStr, out var parsedPort) ? parsedPort : 0;
                await smtp.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.SslOnConnect);

                var username = Environment.GetEnvironmentVariable("EMAIL_USERNAME") ?? _config["Email:Username"];
                var password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD") ?? _config["Email:Password"];
                await smtp.AuthenticateAsync(username, password);

                await smtp.SendAsync(email);
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }
    }
}
