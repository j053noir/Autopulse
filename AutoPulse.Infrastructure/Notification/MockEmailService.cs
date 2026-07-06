using AutoPulse.Application.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace AutoPulse.Infrastructure.Notification
{
    public class MockEmailService : IEmailService
    {
        private readonly ILogger<MockEmailService> _logger;

        public MockEmailService(ILogger<MockEmailService> logger)
        {
            _logger = logger;
        }

        public async Task BulkSendEmailAsync(string from, IEnumerable<string> to, string subject, string body, string? cc, string? bcc)
        {
            _logger.LogInformation("Mock sending bulk email from {From} to {To} with subject {Subject}.", from, string.Join(", ", to), subject);
        }

        public async Task SendEmailAsync(string from, string to, string subject, string body, string? cc, string? bcc)
        {
            _logger.LogInformation("Mock sending email from {From} to {To} with subject {Subject}.", from, to, subject);
        }
    }
}
