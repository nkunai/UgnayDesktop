using UgnayDesktop.Data;
using UgnayDesktop.Models;
using UgnayDesktop.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace UgnayDesktop.Forms
{
    public partial class TeacherDashboard : Form
    {
        private static readonly TimeSpan StudentOnlineWindow = TimeSpan.FromSeconds(30);
        private readonly TextBeeService _textBeeService = new();
        private readonly User _currentTeacher;

        public TeacherDashboard(User currentTeacher)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            _currentTeacher = currentTeacher;
            UdpSensorListener.Shared.SensorReadingReceived += SensorListener_SensorReadingReceived;
            LoadCurrentTeacherProfile();
            LoadStudents();
            UpdateTeacherPhoneLabel();
            ConfigureStudentEntryUi();
        }

        private void LoadCurrentTeacherProfile()
        {
            txtTeacherFullName.Text = _currentTeacher.FullName;
            txtTeacherPhoneSuffix.Text = ExtractPhPhoneDigitsAfterCountryCode(_currentTeacher.TeacherPhoneNumber);
        }

        private static string ExtractPhPhoneDigitsAfterCountryCode(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return string.Empty;
            }

            var rawDigits = new string(phoneNumber.Where(char.IsDigit).ToArray());

            if (rawDigits.StartsWith("63", StringComparison.Ordinal))
            {
                rawDigits = rawDigits.Substring(2);
            }
            else if (rawDigits.StartsWith("0", StringComparison.Ordinal))
            {
                rawDigits = rawDigits.Substring(1);
            }

            if (rawDigits.Length > 10)
            {
                rawDigits = rawDigits.Substring(rawDigits.Length - 10);
            }

            return rawDigits;
        }

        private void txtTeacherPhoneSuffix_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void UpdateTeacherPhoneLabel()
        {
            var phone = string.IsNullOrWhiteSpace(_currentTeacher.TeacherPhoneNumber)
                ? "not set"
                : _currentTeacher.TeacherPhoneNumber;
            lblTeacherPhone.Text = $"Teacher Phone: {phone}";
        }

        private void LoadStudents()
        {
            using var db = new AppDbContext();

            var students = db.Users
                .Where(u => u.Role == "Student")
                .OrderBy(u => u.FullName)
                .Select(u => new { u.Id, u.FullName, u.DeviceId })
                .ToList();

            RenderStudentCards(students.Select(s => (s.Id, s.FullName, s.DeviceId)));
        }

        private void btnSaveProfile_Click(object sender, EventArgs e)
        {
            var currentFullName = _currentTeacher.FullName;
            var currentPhoneDigits = ExtractPhPhoneDigitsAfterCountryCode(_currentTeacher.TeacherPhoneNumber);

            using var dialog = new EditTeacherProfileDialog(currentFullName, currentPhoneDigits);
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            using var db = new AppDbContext();
            var teacher = db.Users.FirstOrDefault(u => u.Id == _currentTeacher.Id && u.Role == "Teacher");
            if (teacher == null)
            {
                MessageBox.Show("Teacher profile not found.", "Profile", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            teacher.FullName = dialog.TeacherFullName;
            teacher.TeacherPhoneNumber = dialog.NormalizedPhoneNumber;

            db.SaveChanges();

            _currentTeacher.FullName = teacher.FullName;
            _currentTeacher.TeacherPhoneNumber = teacher.TeacherPhoneNumber;

            LoadCurrentTeacherProfile();
            UpdateTeacherPhoneLabel();
            MessageBox.Show("Profile updated successfully.", "Profile", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ConfigureStudentEntryUi()
        {
            lblStudentHeader.Text = "Student Cards";
            btnAddStudent.Text = "Add Student";

            InitializeStudentCardsUi();
        }

        private void btnAddStudent_Click(object sender, EventArgs e)
        {
            using var dialog = new AddStudentDialog();
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var fullName = dialog.StudentFullName;
            var age = dialog.StudentAge;
            var sex = dialog.StudentSex;

            if (string.IsNullOrWhiteSpace(sex))
            {
                MessageBox.Show("Fill student name, age, and sex.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var db = new AppDbContext();
            var username = BuildUniqueStudentUsername(db, fullName);
            var deviceId = BuildStudentDeviceId(fullName);

            db.Users.Add(new User
            {
                FullName = fullName,
                Age = age,
                Sex = sex,
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N")),
                Role = "Student",
                DeviceId = deviceId,
            });
            db.SaveChanges();

            LoadStudents();

            MessageBox.Show($"Student added. Assigned device ID: {deviceId}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static string BuildUniqueStudentUsername(AppDbContext db, string fullName)
        {
            var raw = new string(fullName.ToLowerInvariant().Where(c => char.IsLetterOrDigit(c)).ToArray());

            var baseName = string.IsNullOrWhiteSpace(raw) ? "student" : raw;
            var candidate = $"{baseName}_student";
            var suffix = 1;

            while (db.Users.Any(u => u.Username == candidate))
            {
                candidate = $"{baseName}_student{suffix}";
                suffix++;
            }

            return candidate;
        }

        private static string BuildStudentDeviceId(string fullName)
        {
            var clean = new string(fullName.ToLowerInvariant().Where(c => char.IsLetterOrDigit(c)).ToArray());
            if (string.IsNullOrWhiteSpace(clean))
            {
                clean = "student";
            }

            var token = Guid.NewGuid().ToString("N").Substring(0, 4);
            return $"esp32-{clean}-{token}";
        }

        private void SensorListener_SensorReadingReceived(SensorReading reading)
        {
            if (!IsHandleCreated)
            {
                return;
            }

            BeginInvoke(() =>
            {
                UpdateStudentCardFromReading(reading);
            });
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show("Are you sure you want to log out?", "Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                UdpSensorListener.Shared.SensorReadingReceived -= SensorListener_SensorReadingReceived;
                Close();
            }
        }

        private void btnTwilioLink_Click(object sender, EventArgs e)
        {
            try
            {
                _textBeeService.OpenWebhookSetup();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open TextBee webhook page: {ex.Message}", "TextBee", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnTwilioConfigCheck_Click(object sender, EventArgs e)
        {
            var missing = _textBeeService.GetMissingConfigKeys();
            var hasTeacherPhone = !string.IsNullOrWhiteSpace(_currentTeacher.TeacherPhoneNumber);

            if (missing.Count == 0 && hasTeacherPhone)
            {
                MessageBox.Show("TextBee configuration check passed. You can send a test SMS now.", "TextBee", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var issues = new List<string>();
            foreach (var key in missing)
            {
                issues.Add($"Missing env var: {key}");
            }

            if (!hasTeacherPhone)
            {
                issues.Add("Teacher phone number is not set in profile.");
            }

            MessageBox.Show(
                "TextBee configuration is incomplete:" + Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, issues),
                "TextBee Config Check",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private async void btnTwilioTest_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentTeacher.TeacherPhoneNumber))
            {
                MessageBox.Show("Teacher phone number is not set. Update your profile first.", "TextBee", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var missing = _textBeeService.GetMissingConfigKeys();
            if (missing.Count > 0)
            {
                MessageBox.Show("TextBee is not configured. Missing: " + string.Join(", ", missing), "TextBee", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var reference = await _textBeeService.SendTestNotificationToTeacherAsync(_currentTeacher.TeacherPhoneNumber, _currentTeacher.FullName);
                MessageBox.Show($"TextBee test SMS sent successfully. Reference: {reference}", "TextBee", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"TextBee test failed: {ex.Message}", "TextBee", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void btnBpmAlert_Click(object sender, EventArgs e)
        {
            await SendManualAlertAsync(btnBpmAlert, txtBpmAlertMessage, "BPM Alert");
        }
        private async void btnSweatnessAlert_Click(object sender, EventArgs e)
        {
            await SendManualAlertAsync(btnSweatnessAlert, txtSweatnessAlertMessage, "Sweatness Alert");
        }
        private async void btnTemperatureAlert_Click(object sender, EventArgs e)
        {
            await SendManualAlertAsync(btnTemperatureAlert, txtTemperatureAlertMessage, "Temperature Alert");
        }
        private async Task SendManualAlertAsync(Button alertButton, TextBox messageTextBox, string alertName)
        {
            if (string.IsNullOrWhiteSpace(_currentTeacher.TeacherPhoneNumber))
            {
                MessageBox.Show("Teacher phone number is not set. Update your profile first.", "TextBee", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var missing = _textBeeService.GetMissingConfigKeys();
            if (missing.Count > 0)
            {
                MessageBox.Show("TextBee is not configured. Missing: " + string.Join(", ", missing), "TextBee", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var message = messageTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                MessageBox.Show($"{alertName} message cannot be empty.", "TextBee", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                messageTextBox.Focus();
                return;
            }
            alertButton.Enabled = false;
            try
            {
                var reference = await _textBeeService.SendSmsAsync(_currentTeacher.TeacherPhoneNumber, message);
                MessageBox.Show($"{alertName} SMS sent successfully. Reference: {reference}", "TextBee", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{alertName} SMS failed: {ex.Message}", "TextBee", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                alertButton.Enabled = true;
            }
        }
    }
}
