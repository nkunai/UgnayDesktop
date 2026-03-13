namespace UgnayDesktop.Models;

public class OtpChallenge
{
    public int Id { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string CodeHash { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastSentAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? ConsumedAtUtc { get; set; }
    public int AttemptCount { get; set; }
    public int ResendCount { get; set; }
    public string? PendingFullName { get; set; }
    public string? PendingPasswordHash { get; set; }
}
