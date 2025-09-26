using System.Net;
using System.Net.Mail;
using Apointo.Application.Common.Interfaces.Notifications;
using Apointo.Application.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apointo.Infrastructure.Services;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailSettings> settings, ILogger<SmtpEmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        if (!_settings.IsEnabled)
        {
            _logger.LogWarning("Email sending is disabled. Skipping email to {Email}", toEmail);
            return;
        }

        if (string.IsNullOrWhiteSpace(_settings.SmtpHost) || _settings.SmtpPort == 0)
        {
            throw new InvalidOperationException("SMTP configuration is missing. Please configure EmailSettings.");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        message.To.Add(new MailAddress(toEmail));

        using var smtpClient = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
        {
            EnableSsl = _settings.UseSsl,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password)
        };

        using var registration = cancellationToken.Register(static state =>
        {
            if (state is SmtpClient client)
            {
                client.SendAsyncCancel();
            }
        }, smtpClient);

        await smtpClient.SendMailAsync(message);
        _logger.LogInformation("Email sent to {Email} with subject {Subject}", toEmail, subject);
    }
}