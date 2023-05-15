using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types.Mail;

namespace LBPUnion.ProjectLighthouse.Mail;

public class NullMailService : IMailService, IDisposable
{
    public void SendEmail(string recipientAddress, string subject, string body) { }
    public Task<bool> SendEmailAsync(string recipientAddress, string subject, string body) => Task.FromResult(true);
    public void Dispose() => GC.SuppressFinalize(this);
}