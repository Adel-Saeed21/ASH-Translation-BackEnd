using ASH_Translation.Services.Interface;
using System.Text;
using System.Text.Json;

namespace ASH_Translation.Services
{
    public class ResendEmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _fromEmail;

        public ResendEmailService(IConfiguration config, HttpClient httpClient)
        {
            _config = config;
            _httpClient = httpClient;
            
            // Get API key from environment variable or configuration
            _apiKey = Environment.GetEnvironmentVariable("RESEND_API_KEY") ?? _config["Resend:ApiKey"];
            if (string.IsNullOrEmpty(_apiKey))
                throw new Exception("RESEND_API_KEY is not configured. Please set it in .env file or appsettings.json");
            
            // Get from email from environment variable or configuration
            _fromEmail = Environment.GetEnvironmentVariable("EMAIL_FROM") ?? _config["Email:From"] ?? _config["Resend:From"];
            if (string.IsNullOrEmpty(_fromEmail))
                throw new Exception("EMAIL_FROM is not configured. Please set it in .env file or appsettings.json");
            
            // Set Authorization header (BaseAddress and Accept are configured in Program.cs)
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task SendEmailAsync(string to, string subject, string htmlMessage)
        {
            try
            {
                var emailRequest = new
                {
                    from = _fromEmail,
                    to = new[] { to },
                    subject = subject,
                    html = htmlMessage
                };

                var json = JsonSerializer.Serialize(emailRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("emails", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to send email via Resend. Status: {response.StatusCode}, Error: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send email via Resend: {ex.Message}", ex);
            }
        }
    }
}

