namespace UgnayDesktop.Models;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Role { get; set; } = string.Empty; // Admin, Teacher, or Student
    public string FullName { get; set; } = string.Empty;
    public string? TeacherPhoneNumber { get; set; }
    public string? DeviceId { get; set; }
    public int? Age { get; set; }
    public string? Sex { get; set; }
    public string ThemePreference { get; set; } = "Light";
    public bool MustChangePassword { get; set; }
}


