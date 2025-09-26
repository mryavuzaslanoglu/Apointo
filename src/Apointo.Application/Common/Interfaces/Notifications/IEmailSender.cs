namespace Apointo.Application.Common.Interfaces.Notifications;

public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
}