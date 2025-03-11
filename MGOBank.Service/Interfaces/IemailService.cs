namespace MGOBankApp.BLL.Interfaces
{
    public interface IEmailService
    {
        Task SendEmail(string email, string subject, string message);
    }
}