using System.Net;
using System.Net.Mail;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Types.Mail;

namespace LBPUnion.ProjectLighthouse.Mail;

public class SmtpMailSender : IMailSender
{
    public async void SendEmail(MailMessage message)
    {
        using SmtpClient client = new(ServerConfiguration.Instance.Mail.Host, ServerConfiguration.Instance.Mail.Port)
        {
            EnableSsl = ServerConfiguration.Instance.Mail.UseSSL,
            Credentials = new NetworkCredential(ServerConfiguration.Instance.Mail.Username,
                ServerConfiguration.Instance.Mail.Password),
        };
        await client.SendMailAsync(message);
    }
}