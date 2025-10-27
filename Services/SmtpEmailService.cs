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
            email.From.Add(MailboxAddress.Parse(_config["Email:From"]));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = htmlMessage
            };

            using var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync(
                    _config["Email:Host"],
                    int.Parse(_config["Email:Port"]),
                    MailKit.Security.SecureSocketOptions.SslOnConnect
                );

                await smtp.AuthenticateAsync(
                    _config["Email:Username"],
                    _config["Email:Password"]
                );

                await smtp.SendAsync(email);
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }
    }
}
