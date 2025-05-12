namespace Calligraphy.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string email, string subject, string mailBody);
    }
}
