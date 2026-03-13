using System.Security.Cryptography;
using UgnayDesktop.Data;
using UgnayDesktop.Models;
using BCrypt.Net;

namespace UgnayDesktop.Services;

public class AuthService
{
    private const string SignupTeacherPurpose = "SignupTeacher";
    private const string LoginTeacherPurpose = "LoginTeacher";
    private const int OtpLength = 6;
    private const int MaxOtpAttempts = 5;
    private static readonly TimeSpan OtpValidity = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(30);

    private readonly TextBeeService _textBeeService = new();

    public async Task<OtpChallengeResult> BeginTeacherSignupAsync(
        string fullName,
        string username,
        string password,
        string confirmPassword,
        string contactDigits,
        CancellationToken cancellationToken = default)
    {
        fullName = fullName.Trim();
        username = username.Trim();
        contactDigits = NormalizeContactDigits(contactDigits);

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return Failure("Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            return Failure("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return Failure("Password is required.");
        }

        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
        {
            return Failure("Passwords do not match.");
        }

        if (!IsValidContactDigits(contactDigits))
        {
            return Failure("Contact number must be 10 digits starting with 9 after +63.");
        }

        using var db = new AppDbContext();
        CleanupExpiredChallenges(db);

        if (db.Users.Any(u => u.Username == username))
        {
            return Failure("Username already exists.");
        }

        var nowUtc = DateTime.UtcNow;
        InvalidateActiveChallenges(db, SignupTeacherPurpose, username, nowUtc);

        var otpCode = GenerateOtpCode();
        var challenge = new OtpChallenge
        {
            Purpose = SignupTeacherPurpose,
            Username = username,
            PhoneNumber = $"+63{contactDigits}",
            CodeHash = BCrypt.Net.BCrypt.HashPassword(otpCode),
            CreatedAtUtc = nowUtc,
            LastSentAtUtc = nowUtc,
            ExpiresAtUtc = nowUtc.Add(OtpValidity),
            PendingFullName = fullName,
            PendingPasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            AttemptCount = 0,
            ResendCount = 0
        };

        db.OtpChallenges.Add(challenge);
        db.SaveChanges();

        try
        {
            await _textBeeService.SendOtpAsync(challenge.PhoneNumber, otpCode, cancellationToken);
        }
        catch (Exception ex)
        {
            db.OtpChallenges.Remove(challenge);
            db.SaveChanges();
            return Failure($"Unable to send OTP: {ex.Message}");
        }

        return SuccessChallenge(challenge, "OTP sent to the teacher phone number.");
    }

    public CompleteTeacherSignupResult CompleteTeacherSignup(int challengeId, string otpCode)
    {
        using var db = new AppDbContext();
        CleanupExpiredChallenges(db);

        var verification = VerifyChallenge(db, challengeId, SignupTeacherPurpose, otpCode);
        if (!verification.Success)
        {
            return new CompleteTeacherSignupResult { Success = false, Message = verification.Message };
        }

        var challenge = verification.Challenge!;

        if (string.IsNullOrWhiteSpace(challenge.PendingFullName) || string.IsNullOrWhiteSpace(challenge.PendingPasswordHash))
        {
            challenge.ConsumedAtUtc = DateTime.UtcNow;
            db.SaveChanges();
            return new CompleteTeacherSignupResult { Success = false, Message = "Signup challenge is incomplete. Start again." };
        }

        if (db.Users.Any(u => u.Username == challenge.Username))
        {
            challenge.ConsumedAtUtc = DateTime.UtcNow;
            db.SaveChanges();
            return new CompleteTeacherSignupResult { Success = false, Message = "Username already exists." };
        }

        db.Users.Add(new User
        {
            FullName = challenge.PendingFullName,
            Username = challenge.Username,
            PasswordHash = challenge.PendingPasswordHash,
            Role = "Teacher",
            TeacherPhoneNumber = challenge.PhoneNumber
        });

        challenge.ConsumedAtUtc = DateTime.UtcNow;
        db.SaveChanges();

        return new CompleteTeacherSignupResult
        {
            Success = true,
            Message = "Teacher account created. You can now sign in.",
            Username = challenge.Username
        };
    }

    public async Task<BeginLoginResult> BeginLoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        username = username.Trim();
        password = password.Trim();

        using var db = new AppDbContext();
        CleanupExpiredChallenges(db);

        var user = db.Users.FirstOrDefault(u => u.Username == username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return new BeginLoginResult
            {
                Status = AuthLoginStatus.InvalidCredentials,
                Message = "Invalid username or password."
            };
        }

        if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return new BeginLoginResult
            {
                Status = AuthLoginStatus.Authenticated,
                Message = "Login successful.",
                User = user
            };
        }

        if (!string.Equals(user.Role, "Teacher", StringComparison.OrdinalIgnoreCase))
        {
            return new BeginLoginResult
            {
                Status = AuthLoginStatus.Blocked,
                Message = "Student accounts are for ESP32 assignment and cannot sign in to the desktop dashboard."
            };
        }

        if (string.IsNullOrWhiteSpace(user.TeacherPhoneNumber))
        {
            return new BeginLoginResult
            {
                Status = AuthLoginStatus.Error,
                Message = "Teacher phone number is not set. Contact an admin."
            };
        }

        var nowUtc = DateTime.UtcNow;
        InvalidateActiveChallenges(db, LoginTeacherPurpose, user.Username, nowUtc);

        var otpCode = GenerateOtpCode();
        var challenge = new OtpChallenge
        {
            Purpose = LoginTeacherPurpose,
            Username = user.Username,
            PhoneNumber = user.TeacherPhoneNumber,
            CodeHash = BCrypt.Net.BCrypt.HashPassword(otpCode),
            CreatedAtUtc = nowUtc,
            LastSentAtUtc = nowUtc,
            ExpiresAtUtc = nowUtc.Add(OtpValidity),
            AttemptCount = 0,
            ResendCount = 0
        };

        db.OtpChallenges.Add(challenge);
        db.SaveChanges();

        try
        {
            await _textBeeService.SendOtpAsync(challenge.PhoneNumber, otpCode, cancellationToken);
        }
        catch (Exception ex)
        {
            db.OtpChallenges.Remove(challenge);
            db.SaveChanges();
            return new BeginLoginResult
            {
                Status = AuthLoginStatus.Error,
                Message = $"Unable to send OTP: {ex.Message}"
            };
        }

        return new BeginLoginResult
        {
            Status = AuthLoginStatus.OtpRequired,
            Message = "OTP sent to the teacher phone number.",
            ChallengeId = challenge.Id,
            MaskedPhoneNumber = MaskPhoneNumber(challenge.PhoneNumber),
            ExpiresAtUtc = challenge.ExpiresAtUtc,
            ResendAvailableAtUtc = challenge.LastSentAtUtc.Add(ResendCooldown)
        };
    }

    public CompleteTeacherLoginOtpResult CompleteTeacherLoginOtp(int challengeId, string otpCode)
    {
        using var db = new AppDbContext();
        CleanupExpiredChallenges(db);

        var verification = VerifyChallenge(db, challengeId, LoginTeacherPurpose, otpCode);
        if (!verification.Success)
        {
            return new CompleteTeacherLoginOtpResult { Success = false, Message = verification.Message };
        }

        var challenge = verification.Challenge!;
        var user = db.Users.FirstOrDefault(u => u.Username == challenge.Username && u.Role == "Teacher");
        if (user == null)
        {
            challenge.ConsumedAtUtc = DateTime.UtcNow;
            db.SaveChanges();
            return new CompleteTeacherLoginOtpResult { Success = false, Message = "Teacher account not found." };
        }

        challenge.ConsumedAtUtc = DateTime.UtcNow;
        db.SaveChanges();

        return new CompleteTeacherLoginOtpResult
        {
            Success = true,
            Message = "OTP verified.",
            User = user
        };
    }

    public Task<OtpChallengeResult> ResendTeacherSignupOtpAsync(int challengeId, CancellationToken cancellationToken = default)
    {
        return ResendOtpAsync(challengeId, SignupTeacherPurpose, cancellationToken);
    }

    public Task<OtpChallengeResult> ResendTeacherLoginOtpAsync(int challengeId, CancellationToken cancellationToken = default)
    {
        return ResendOtpAsync(challengeId, LoginTeacherPurpose, cancellationToken);
    }

    private async Task<OtpChallengeResult> ResendOtpAsync(int challengeId, string purpose, CancellationToken cancellationToken)
    {
        using var db = new AppDbContext();
        CleanupExpiredChallenges(db);

        var challenge = db.OtpChallenges.FirstOrDefault(c => c.Id == challengeId && c.Purpose == purpose);
        if (challenge == null)
        {
            return Failure("OTP challenge was not found. Start again.");
        }

        if (challenge.ConsumedAtUtc != null)
        {
            return Failure("OTP challenge is no longer active. Start again.");
        }

        var nowUtc = DateTime.UtcNow;
        var resendAvailableAtUtc = challenge.LastSentAtUtc.Add(ResendCooldown);
        if (resendAvailableAtUtc > nowUtc)
        {
            var waitSeconds = Math.Max(1, (int)Math.Ceiling((resendAvailableAtUtc - nowUtc).TotalSeconds));
            return Failure($"Wait {waitSeconds} seconds before requesting a new OTP.");
        }

        var otpCode = GenerateOtpCode();
        challenge.CodeHash = BCrypt.Net.BCrypt.HashPassword(otpCode);
        challenge.LastSentAtUtc = nowUtc;
        challenge.ExpiresAtUtc = nowUtc.Add(OtpValidity);
        challenge.AttemptCount = 0;
        challenge.ResendCount += 1;
        db.SaveChanges();

        try
        {
            await _textBeeService.SendOtpAsync(challenge.PhoneNumber, otpCode, cancellationToken);
        }
        catch (Exception ex)
        {
            return Failure($"Unable to resend OTP: {ex.Message}");
        }

        return SuccessChallenge(challenge, "A new OTP was sent.");
    }

    private static void CleanupExpiredChallenges(AppDbContext db)
    {
        var nowUtc = DateTime.UtcNow;
        var staleChallenges = db.OtpChallenges
            .Where(c => c.ConsumedAtUtc == null && c.ExpiresAtUtc < nowUtc)
            .ToList();

        foreach (var challenge in staleChallenges)
        {
            challenge.ConsumedAtUtc = nowUtc;
        }

        if (staleChallenges.Count > 0)
        {
            db.SaveChanges();
        }
    }

    private static void InvalidateActiveChallenges(AppDbContext db, string purpose, string username, DateTime nowUtc)
    {
        var activeChallenges = db.OtpChallenges
            .Where(c => c.Purpose == purpose && c.Username == username && c.ConsumedAtUtc == null)
            .ToList();

        foreach (var challenge in activeChallenges)
        {
            challenge.ConsumedAtUtc = nowUtc;
        }
    }

    private static (bool Success, string Message, OtpChallenge? Challenge) VerifyChallenge(AppDbContext db, int challengeId, string purpose, string otpCode)
    {
        var challenge = db.OtpChallenges.FirstOrDefault(c => c.Id == challengeId && c.Purpose == purpose);
        if (challenge == null)
        {
            return (false, "OTP challenge was not found. Start again.", null);
        }

        if (challenge.ConsumedAtUtc != null)
        {
            return (false, "OTP challenge is no longer active. Start again.", null);
        }

        var nowUtc = DateTime.UtcNow;
        if (challenge.ExpiresAtUtc < nowUtc)
        {
            challenge.ConsumedAtUtc = nowUtc;
            db.SaveChanges();
            return (false, "OTP has expired. Request a new code.", null);
        }

        if (challenge.AttemptCount >= MaxOtpAttempts)
        {
            challenge.ConsumedAtUtc = nowUtc;
            db.SaveChanges();
            return (false, "Too many incorrect OTP attempts. Start again.", null);
        }

        otpCode = otpCode.Trim();
        if (otpCode.Length != OtpLength || otpCode.Any(ch => !char.IsDigit(ch)))
        {
            return (false, "OTP must be a 6-digit code.", null);
        }

        if (!BCrypt.Net.BCrypt.Verify(otpCode, challenge.CodeHash))
        {
            challenge.AttemptCount += 1;
            if (challenge.AttemptCount >= MaxOtpAttempts)
            {
                challenge.ConsumedAtUtc = nowUtc;
                db.SaveChanges();
                return (false, "Too many incorrect OTP attempts. Start again.", null);
            }

            db.SaveChanges();
            return (false, "Incorrect OTP. Try again.", null);
        }

        return (true, string.Empty, challenge);
    }

    private static OtpChallengeResult SuccessChallenge(OtpChallenge challenge, string message)
    {
        return new OtpChallengeResult
        {
            Success = true,
            Message = message,
            ChallengeId = challenge.Id,
            MaskedPhoneNumber = MaskPhoneNumber(challenge.PhoneNumber),
            ExpiresAtUtc = challenge.ExpiresAtUtc,
            ResendAvailableAtUtc = challenge.LastSentAtUtc.Add(ResendCooldown)
        };
    }

    private static OtpChallengeResult Failure(string message)
    {
        return new OtpChallengeResult
        {
            Success = false,
            Message = message
        };
    }

    private static string GenerateOtpCode()
    {
        return RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
    }

    private static string NormalizeContactDigits(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var digits = new string(input.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("63", StringComparison.Ordinal) && digits.Length >= 12)
        {
            digits = digits.Substring(2);
        }
        else if (digits.StartsWith("0", StringComparison.Ordinal) && digits.Length >= 11)
        {
            digits = digits.Substring(1);
        }

        if (digits.Length > 10)
        {
            digits = digits.Substring(digits.Length - 10);
        }

        return digits;
    }

    private static bool IsValidContactDigits(string digits)
    {
        return digits.Length == 10 && digits[0] == '9' && digits.All(char.IsDigit);
    }

    private static string MaskPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return "your phone";
        }

        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
        if (digits.Length < 4)
        {
            return phoneNumber;
        }

        var suffix = digits[^4..];
        return $"+63******{suffix}";
    }
}
