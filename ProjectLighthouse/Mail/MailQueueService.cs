using System;
using System.Collections.Concurrent;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Mail;

namespace LBPUnion.ProjectLighthouse.Mail;

public class MailQueueService : IMailService, IDisposable
{
    private readonly MailAddress fromAddress;

    private readonly ConcurrentQueue<EmailEntry> emailQueue = new();

    private readonly SemaphoreSlim emailSemaphore = new(0);

    private bool stopSignal;

    private readonly Task emailThread;

    private readonly IMailSender mailSender;

    public MailQueueService(IMailSender mailSender)
    {
        if (!ServerConfiguration.Instance.Mail.MailEnabled) return;

        this.mailSender = mailSender;

        this.fromAddress = new MailAddress(ServerConfiguration.Instance.Mail.FromAddress, ServerConfiguration.Instance.Mail.FromName);
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
                this.mailSender.SendEmail(entry.Message);
                entry.Result.SetResult(true);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to send email: {e}", LogArea.Email);
                entry.Result.SetResult(false);
            }
        }
    }

    public void Dispose()
    {
        this.stopSignal = true;
        if (this.emailThread != null)
        {
            this.emailThread.Wait();
            this.emailThread.Dispose();
        }
        GC.SuppressFinalize(this);
    }

    public void SendEmail(string recipientAddress, string subject, string body)
    {
        TaskCompletionSource<bool> resultTask = new();
        this.SendEmail(recipientAddress, subject, body, resultTask);
    }

    public Task<bool> SendEmailAsync(string recipientAddress, string subject, string body)
    {
        TaskCompletionSource<bool> resultTask = new();
        this.SendEmail(recipientAddress, subject, body, resultTask);
        return resultTask.Task;
    }

    private void SendEmail(string recipientAddress, string subject, string body, TaskCompletionSource<bool> resultTask)
    {
        if (!ServerConfiguration.Instance.Mail.MailEnabled)
        {
            resultTask.SetResult(false);
            return;
        }

        MailMessage message = new(this.fromAddress, new MailAddress(recipientAddress))
        {
            Subject = subject,
            Body = body,
        };

        this.emailQueue.Enqueue(new EmailEntry(message, resultTask));
        this.emailSemaphore.Release();
    }

    private class EmailEntry
    {
        public MailMessage Message { get; }
        public TaskCompletionSource<bool> Result { get; }

        public EmailEntry(MailMessage message, TaskCompletionSource<bool> result)
        {
            this.Message = message;
            this.Result = result;
        }
    } 
}