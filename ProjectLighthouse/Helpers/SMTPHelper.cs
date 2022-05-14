using System;
using System.Net;
using System.Net.Mail;
using LBPUnion.ProjectLighthouse.Types.Settings;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class SMTPHelper
{
    private static readonly SmtpClient client;
    private static readonly MailAddress fromAddress;

    static SMTPHelper()
    {
        if (!ServerConfiguration.Instance.Mail.MailEnabled) return;

        client = new SmtpClient(ServerConfiguration.Instance.Mail.Host, ServerConfiguration.Instance.Mail.Port)
        {
            EnableSsl = ServerConfiguration.Instance.Mail.UseSSL,
            Credentials = new NetworkCredential(ServerConfiguration.Instance.Mail.FromAddress, ServerConfiguration.Instance.Mail.Password),
        };

        fromAddress = new MailAddress(ServerConfiguration.Instance.Mail.FromAddress, ServerConfiguration.Instance.Mail.FromName);
    }

    public static bool SendEmail(string recipientAddress, string subject, string body)
    {
        if (!ServerConfiguration.Instance.Mail.MailEnabled) return false;

        MailMessage message = new(fromAddress, new MailAddress(recipientAddress))
        {
            Subject = subject,
            Body = body,
        };

        try
        {
            client.Send(message);
        }
        catch(Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

        return true;
    }
}