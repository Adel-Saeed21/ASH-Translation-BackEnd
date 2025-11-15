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
            if (string.IsNullOrEmpty(from))
                throw new Exception("EMAIL_FROM is not configured. Please set it in .env file or appsettings.json");
            
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
                int port = int.TryParse(portStr, out var parsedPort) ? parsedPort : 587;
                
                if (string.IsNullOrEmpty(host))
                    throw new Exception("EMAIL_HOST is not configured. Please set it in .env file or appsettings.json");
                
                // Office 365 works best with Auto - it will negotiate the best security option
                // Port 465 uses SSL (SslOnConnect), port 587 uses STARTTLS (StartTls)
                var securityOption = port == 465 
                    ? MailKit.Security.SecureSocketOptions.SslOnConnect 
                    : MailKit.Security.SecureSocketOptions.Auto; // Auto works best for Office 365
                
                // Set timeout to 30 seconds (default is 10 seconds which might be too short)
                smtp.Timeout = 30000;
                
                await smtp.ConnectAsync(host, port, securityOption);

                var username = Environment.GetEnvironmentVariable("EMAIL_USERNAME") ?? _config["Email:Username"];
                var password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD") ?? _config["Email:Password"];
                
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    throw new Exception("EMAIL_USERNAME and EMAIL_PASSWORD must be configured in .env file or appsettings.json");
                
                await smtp.AuthenticateAsync(username, password);

                await smtp.SendAsync(email);
            }
            catch (Exception ex)
            {
                var host = Environment.GetEnvironmentVariable("EMAIL_HOST") ?? _config["Email:Host"];
                var portStr = Environment.GetEnvironmentVariable("EMAIL_PORT") ?? _config["Email:Port"];
                throw new Exception($"Failed to send email. Host: {host}, Port: {portStr}, Error: {ex.Message}", ex);
            }
            finally
            {
                if (smtp.IsConnected)
                {
                    await smtp.DisconnectAsync(true);
                }
            }
        }
    }
}
