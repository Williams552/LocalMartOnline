using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;

namespace LocalMartOnline.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUser = _configuration["Email:Username"];
            var smtpPass = _configuration["Email:Password"];
            var fromEmail = _configuration["Email:From"];
            if (string.IsNullOrWhiteSpace(fromEmail))
            {
                throw new InvalidOperationException("Sender email address (Email:From) is not configured.");
            }

            var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") 
                ?? _configuration["App:BaseUrl"] 
                ?? "https://localmartonline-1.onrender.com/";

            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.EnableSsl = true; // Bắt buộc phải bật SSL/TLS với Gmail
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                var mail = new MailMessage(fromEmail, to, subject, body);
                mail.IsBodyHtml = true;
                await client.SendMailAsync(mail);
            }
        }
    }
}
