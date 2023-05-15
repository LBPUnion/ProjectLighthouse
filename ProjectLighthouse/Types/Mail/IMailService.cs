using System.Threading.Tasks;

namespace LBPUnion.ProjectLighthouse.Types.Mail;

public interface IMailService
{
    public void SendEmail(string recipientAddress, string subject, string body);
    public Task<bool> SendEmailAsync(string recipientAddress, string subject, string body);
}