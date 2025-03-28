 using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using ScanGuard.BLL.Interfaces;
using ScanGuard.BLL.Utilities;
using Microsoft.Extensions.Configuration;

namespace ScanGuard.BLL.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _appMail;
        private readonly string _appMailPassword;
        private readonly string _smtpServer;
        private readonly int _port;

        public EmailService(IConfiguration configuration)
        {
            _appMail = configuration["EmailService:Email"]!;
            _appMailPassword = configuration["EmailService:Password"]!;
            _smtpServer = configuration["EmailService:SmtpServer"]!;
            _port = int.Parse(configuration["EmailService:Port"]!);
        }

        public async Task SendEmail(string email, string subject, string message)
        {
            var client = new SmtpClient(_smtpServer, _port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_appMail, _appMailPassword)
            };
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_appMail),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);
            await client.SendMailAsync(mailMessage);
        }

        public async Task SendSecurityAlertEmail(string userEmail, string userName, string originalIp, string newIp)
        {
            var message = EmailTemplates.GetSecurityAlertMessage(userName, originalIp, newIp);
            await SendEmail(userEmail, "Security Alert: Unusual Login Activity", message);
        }
    }
}
