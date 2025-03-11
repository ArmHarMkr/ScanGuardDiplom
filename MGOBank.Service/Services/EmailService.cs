using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using MGOBankApp.BLL.Interfaces;

namespace MGOBankApp.BLL.Services
{
    public class EmailService : IEmailService
    {
        public async Task SendEmail(string email, string subject, string message)
        {
            var AppMail = "noreply.scanguard@gmail.com";
            var AppMailPassword = "Scanguard123";
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(AppMail,AppMailPassword)
            };
            using var mailMessage = new MailMessage
            {
                From = new MailAddress("your-email@gmail.com"),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
        }
    }
}
