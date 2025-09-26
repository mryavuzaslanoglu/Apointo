namespace Apointo.Application.Configuration;

public sealed class EmailSettings
{
    public const string SectionName = "EmailSettings";
    public bool IsEnabled { get; init; }
    public string FromName { get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;
    public string SmtpHost { get; init; } = string.Empty;
    public int SmtpPort { get; init; }
    public bool UseSsl { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string PasswordResetUrl { get; init; } = string.Empty;
}