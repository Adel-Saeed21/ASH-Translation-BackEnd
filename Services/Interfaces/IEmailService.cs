namespace ASH_Translation.Services.Interface
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlMessage);
    }
}
