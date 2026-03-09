using System.Collections.Concurrent;
using UgnayDesktop.Models;

namespace UgnayDesktop.Services;

public sealed class EmailMfaService
{
    private readonly IEmailVerificationSender _sender;
    private readonly ConcurrentDictionary<string, ChallengeState> _challenges = new();

    public EmailMfaService(IEmailVerificationSender sender)
    {
        _sender = sender;
    }

    public bool IsMfaRequired(User user)
    {
        var mfaRole = string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase)
            || string.Equals(user.Role, "Teacher", StringComparison.OrdinalIgnoreCase);

        return mfaRole && !string.IsNullOrWhiteSpace(user.Email);
    }

    public BeginChallengeResult BeginChallenge(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Email))
        {
            return new BeginChallengeResult(false, string.Empty, string.Empty, DateTime.UtcNow,
                "No email is configured for this account.");
        }

        var code = Random.Shared.Next(100000, 999999).ToString();
        var nowUtc = DateTime.UtcNow;
        var expiresAtUtc = nowUtc.AddMinutes(5);
        var challengeId = Guid.NewGuid().ToString("N");

        _challenges[challengeId] = new ChallengeState(user.Id, code, expiresAtUtc, 0);

        _sender.SendCode(user.Email, code, expiresAtUtc);

        return new BeginChallengeResult(
            true,
            challengeId,
            MaskedEmail: MaskEmail(user.Email),
            expiresAtUtc,
            string.Empty);
    }

    public VerifyChallengeResult VerifyCode(string challengeId, int userId, string code)
    {
        if (!_challenges.TryGetValue(challengeId, out var challenge))
        {
            return new VerifyChallengeResult(false, false, "Verification session was not found. Please login again.");
        }

        if (challenge.UserId != userId)
        {
            return new VerifyChallengeResult(false, false, "Verification session is invalid for this user.");
        }

        if (DateTime.UtcNow > challenge.ExpiresAtUtc)
        {
            _challenges.TryRemove(challengeId, out _);
            return new VerifyChallengeResult(false, true, "The verification code has expired.");
        }

        if (!string.Equals(challenge.Code, code, StringComparison.Ordinal))
        {
            var attempts = challenge.FailedAttempts + 1;

            if (attempts >= 5)
            {
                _challenges.TryRemove(challengeId, out _);
                return new VerifyChallengeResult(false, false, "Too many invalid attempts. Please login again.");
            }

            _challenges[challengeId] = challenge with { FailedAttempts = attempts };
            return new VerifyChallengeResult(false, false, "Invalid verification code.");
        }

        _challenges.TryRemove(challengeId, out _);
        return new VerifyChallengeResult(true, false, string.Empty);
    }

    private static string MaskEmail(string email)
    {
        var atIndex = email.IndexOf('@');
        if (atIndex <= 1)
            return "***";

        var localPart = email[..atIndex];
        var domain = email[(atIndex + 1)..];

        var visible = localPart.Length <= 2 ? 1 : 2;
        var maskedCount = Math.Max(1, localPart.Length - visible);
        return $"{localPart[..visible]}{new string('*', maskedCount)}@{domain}";
    }

    private sealed record ChallengeState(int UserId, string Code, DateTime ExpiresAtUtc, int FailedAttempts);

    public sealed record BeginChallengeResult(
        bool IsSuccess,
        string ChallengeId,
        string MaskedEmail,
        DateTime ExpiresAtUtc,
        string ErrorMessage);

    public sealed record VerifyChallengeResult(bool IsSuccess, bool IsExpired, string ErrorMessage);
}
