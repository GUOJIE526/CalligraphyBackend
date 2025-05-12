using Calligraphy.Services.Interfaces;
using System.Net;
using System.Net.Mail;

namespace Calligraphy.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;
        public SmtpEmailService(IConfiguration config)
        {
            _config = config;
        }
        public async Task SendAsync(string email, string subject, string mailBody)
        {
            using (var client = new SmtpClient())
            {
                client.Host = _config["Smtp:Host"];
                client.Port = int.Parse(_config["Smtp:Port"]);
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(_config["Smtp:From"], _config["Smtp:Password"]);
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_config["Smtp:From"]),
                    Subject = subject,
                    Body = mailBody,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(email);
                await client.SendMailAsync(mailMessage);
            }
        }
    }
}
