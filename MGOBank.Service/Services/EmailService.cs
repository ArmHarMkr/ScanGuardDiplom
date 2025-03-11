using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using MGOBankApp.BLL.Interfaces;
using MGOBankApp.BLL.Utilities;

namespace MGOBankApp.BLL.Services
{
    public class EmailService : IEmailService
    {
        public async Task SendEmail(string email, string subject, string message)
        {
            var AppMail = "zadref45@gmail.com";
            var AppMailPassword = "fncz lzjt nnpg vcdb";
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(AppMail,AppMailPassword)
            };
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(AppMail),
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
