using System.Net.Mail;

namespace LBPUnion.ProjectLighthouse.Types.Mail;

public interface IMailSender
{
    public void SendEmail(MailMessage message);
}