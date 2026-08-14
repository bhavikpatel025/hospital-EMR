using System.Threading.Tasks;

namespace EMR.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink);
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
    }
}
