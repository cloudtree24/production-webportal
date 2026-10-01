using HCMPortal.Models;

namespace HCMPortal.Interface
{
    public interface IEmailService
    {
        Task<ResponseModel> SendEmailAsync(string toEmail, string toName, string subject, string textContent, string htmlContent = null);
    }
}
