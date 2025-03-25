namespace ScanGuard.BLL.Interfaces
{
    public interface IEmailService
    {
        Task SendEmail(string email, string subject, string message);
        Task SendSecurityAlertEmail(string userEmail, string userName, string originalIp, string newIp);
    }
}