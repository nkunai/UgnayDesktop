namespace UgnayDesktop.Models;

public enum AlertOutboxStatus
{
    Pending = 0,
    Processing = 1,
    Sent = 2,
    Failed = 3,
}

public class AlertOutboxMessage
{
    public int Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public string Source { get; set; } = string.Empty;
    public string DeviceId { get; set; } = "unknown";
    public string Severity { get; set; } = "Warning";
    public string Message { get; set; } = string.Empty;
    public string DedupKey { get; set; } = string.Empty;

    public AlertOutboxStatus Status { get; set; } = AlertOutboxStatus.Pending;
    public int AttemptCount { get; set; }
    public DateTime? LastAttemptAtUtc { get; set; }
    public DateTime? NextAttemptAtUtc { get; set; }
    public DateTime? SentAtUtc { get; set; }
    public string? LastError { get; set; }

    public string? RecipientPhoneNumber { get; set; }
    public string? ProviderMessageId { get; set; }

    public string PayloadJson { get; set; } = "{}";
}
