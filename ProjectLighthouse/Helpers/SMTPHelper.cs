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
        if (!ServerSettings.Instance.SMTPEnabled) return;

        client = new SmtpClient(ServerSettings.Instance.SMTPHost, ServerSettings.Instance.SMTPPort)
        {
            EnableSsl = ServerSettings.Instance.SMTPSsl,
            Credentials = new NetworkCredential(ServerSettings.Instance.SMTPFromAddress, ServerSettings.Instance.SMTPPassword),
        };

        fromAddress = new MailAddress(ServerSettings.Instance.SMTPFromAddress, ServerSettings.Instance.SMTPFromName);
    }

    public static bool SendEmail(string recipientAddress, string subject, string body)
    {
        if (!ServerSettings.Instance.SMTPEnabled) return false;

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