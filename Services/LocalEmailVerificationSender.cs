using System.Diagnostics;

namespace UgnayDesktop.Services;

public sealed class LocalEmailVerificationSender : IEmailVerificationSender
{
    private readonly string _logPath;

    public LocalEmailVerificationSender(string? logPath = null)
    {
        _logPath = logPath ?? Path.Combine(AppContext.BaseDirectory, "mfa-email-preview.log");
    }

    public void SendCode(string recipientEmail, string code, DateTime expiresAtUtc)
    {
        var line = $"[{DateTime.UtcNow:O}] To={recipientEmail}; Code={code}; ExpiresUtc={expiresAtUtc:O}";
        File.AppendAllText(_logPath, line + Environment.NewLine);
        Debug.WriteLine($"[MFA] {line}");
    }
}
