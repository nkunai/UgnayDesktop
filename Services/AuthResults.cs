using UgnayDesktop.Models;

namespace UgnayDesktop.Services;

public enum AuthLoginStatus
{
    InvalidCredentials,
    Authenticated,
    OtpRequired,
    Blocked,
    Error
}

public sealed class OtpChallengeResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public int? ChallengeId { get; init; }
    public string? MaskedPhoneNumber { get; init; }
    public DateTime? ExpiresAtUtc { get; init; }
    public DateTime? ResendAvailableAtUtc { get; init; }
}

public sealed class CompleteTeacherSignupResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? Username { get; init; }
}

public sealed class BeginLoginResult
{
    public AuthLoginStatus Status { get; init; }
    public string Message { get; init; } = string.Empty;
    public User? User { get; init; }
    public int? ChallengeId { get; init; }
    public string? MaskedPhoneNumber { get; init; }
    public DateTime? ExpiresAtUtc { get; init; }
    public DateTime? ResendAvailableAtUtc { get; init; }
}

public sealed class CompleteTeacherLoginOtpResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public User? User { get; init; }
}
