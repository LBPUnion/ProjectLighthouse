using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Logging;

namespace LBPUnion.ProjectLighthouse.Helpers;

public class SMTPHelper
{

    internal static readonly SMTPHelper Instance = new();

    private readonly MailAddress fromAddress;

    private readonly ConcurrentQueue<EmailEntry> emailQueue = new();

    private readonly SemaphoreSlim emailSemaphore = new(0);

    private bool stopSignal;

    private readonly Task emailThread;

    private SMTPHelper()
    {
        if (!ServerConfiguration.Instance.Mail.MailEnabled) return;

        this.fromAddress = new MailAddress(ServerConfiguration.Instance.Mail.FromAddress, ServerConfiguration.Instance.Mail.FromName);

        this.stopSignal = false;
        this.emailThread = Task.Factory.StartNew(this.EmailQueue);
    }

    private async void EmailQueue()
    {
        while (!this.stopSignal)
        {
            await this.emailSemaphore.WaitAsync();
            if (!this.emailQueue.TryDequeue(out EmailEntry entry)) continue;

            try
            {
                using SmtpClient client = new(ServerConfiguration.Instance.Mail.Host, ServerConfiguration.Instance.Mail.Port)
                {
                    EnableSsl = ServerConfiguration.Instance.Mail.UseSSL,
                    Credentials = new NetworkCredential(ServerConfiguration.Instance.Mail.Username, ServerConfiguration.Instance.Mail.Password),
                };
                await client.SendMailAsync(entry.Message);
                entry.Result.SetResult(true);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to send email: {e}", LogArea.Email);
                entry.Result.SetResult(false);
            }
        }
    }

    public static void Dispose()
    {
        Instance.stopSignal = true;
        Instance.emailThread.Wait();
        Instance.emailThread.Dispose();
    }

    public static void SendEmail(string recipientAddress, string subject, string body)
    {
        TaskCompletionSource<bool> resultTask = new();
        Instance.SendEmail(recipientAddress, subject, body, resultTask);
    }

    public static Task<bool> SendEmailAsync(string recipientAddress, string subject, string body)
    {
        TaskCompletionSource<bool> resultTask = new();
        Instance.SendEmail(recipientAddress, subject, body, resultTask);
        return resultTask.Task;
    }

    public void SendEmail(string recipientAddress, string subject, string body, TaskCompletionSource<bool> resultTask)
    {
        if (!ServerConfiguration.Instance.Mail.MailEnabled)
        {
            resultTask.SetResult(false);
            return;
        }

        MailMessage message = new(Instance.fromAddress, new MailAddress(recipientAddress))
        {
            Subject = subject,
            Body = body,
        };

        this.emailQueue.Enqueue(new EmailEntry(message, resultTask));
        this.emailSemaphore.Release();
    }

    internal class EmailEntry
    {
        public MailMessage Message { get; set; }
        public TaskCompletionSource<bool> Result { get; set; }

        public EmailEntry(MailMessage message, TaskCompletionSource<bool> result)
        {
            this.Message = message;
            this.Result = result;
        }
    } 
}