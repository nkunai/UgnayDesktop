using UgnayDesktop.Data;
using UgnayDesktop.Models;

namespace UgnayDesktop.Services;

public class AuthService
{
    public User? ValidateCredentials(string username, string password)
    {
        using var db = new AppDbContext();

        var user = db.Users.FirstOrDefault(u => u.Username == username);

        if (user == null)
            return null;

        bool valid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

        return valid ? user : null;
    }

    public User? Login(string username, string password)
    {
        return ValidateCredentials(username, password);
    }

    public bool TrySetNewPassword(int userId, string newPassword, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (!ValidatePasswordPolicy(newPassword, out errorMessage))
        {
            return false;
        }

        using var db = new AppDbContext();
        var user = db.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
        {
            errorMessage = "User not found.";
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.MustChangePassword = false;
        db.SaveChanges();
        return true;
    }

    private static bool ValidatePasswordPolicy(string password, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(password))
        {
            errorMessage = "Password is required.";
            return false;
        }

        if (password.Length < 8)
        {
            errorMessage = "Password must be at least 8 characters.";
            return false;
        }

        if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit))
        {
            errorMessage = "Password must include at least one letter and one number.";
            return false;
        }

        return true;
    }
}

