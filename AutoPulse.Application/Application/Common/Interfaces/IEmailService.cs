namespace AutoPulse.Application.Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string from, string to, string subject, string body, string? cc = null, string? bcc = null);
        Task BulkSendEmailAsync(string from, IEnumerable<string> to, string subject, string body, string? cc = null, string? bcc = null);
    }
}
