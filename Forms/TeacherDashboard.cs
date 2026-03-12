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
        private readonly TwilioService _twilioService = new();
        private readonly User _currentTeacher;

        private int? _selectedStudentId;
        private string? _selectedStudentName;
        private string? _selectedStudentDeviceId;

        public TeacherDashboard(User currentTeacher)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            _currentTeacher = currentTeacher;
            UdpSensorListener.Shared.SensorReadingReceived += SensorListener_SensorReadingReceived;
            LoadCurrentTeacherProfile();
            LoadStudents();
            ResetSelectedStudentDisplay();
            UpdateTeacherPhoneLabel();
            InitializeGestureStage3Ui();
        }

        private void LoadCurrentTeacherProfile()
        {
            txtTeacherFullName.Text = _currentTeacher.FullName;
            txtTeacherPhoneSuffix.Text = ExtractPhPhoneSuffix(_currentTeacher.TeacherPhoneNumber);
        }

        private static string ExtractPhPhoneSuffix(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return string.Empty;
            }

            var raw = phoneNumber.Trim();
            if (raw.StartsWith("+639", StringComparison.Ordinal))
            {
                raw = raw.Substring(4);
            }
            else if (raw.StartsWith("639", StringComparison.Ordinal))
            {
                raw = raw.Substring(3);
            }
            else if (raw.StartsWith("09", StringComparison.Ordinal))
            {
                raw = raw.Substring(2);
            }

            var digits = new string(raw.Where(char.IsDigit).ToArray());
            if (digits.Length > 9)
            {
                digits = digits.Substring(digits.Length - 9);
            }

            return digits;
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

            dgvStudents.DataSource = db.Users
                .Where(u => u.Role == "Student")
                .OrderBy(u => u.FullName)
                .Select(u => new
                {
                    u.Id,
                    Name = u.FullName,
                    u.Age,
                    u.Sex,
                    u.DeviceId
                })
                .ToList();

            if (dgvStudents.Columns["DeviceId"] != null)
            {
                dgvStudents.Columns["DeviceId"]!.Visible = false;
            }
        }

        private void LoadSensorReadingsForSelectedStudent()
        {
            if (string.IsNullOrWhiteSpace(_selectedStudentDeviceId))
            {
                dgvSensorReadings.DataSource = null;
                dgvSensorReadings.Visible = false;
                return;
            }

            using var db = new AppDbContext();
            dgvSensorReadings.DataSource = db.SensorReadings
                .Where(r => r.DeviceId == _selectedStudentDeviceId)
                .OrderByDescending(r => r.ReceivedAtUtc)
                .Take(30)
                .Select(r => new
                {
                    r.ReceivedAtUtc,
                    Flex = r.FlexValue,
                    TempC = r.BodyTemperatureC,
                    Gsr = r.GsrValue,
                    HR = r.HeartRate,
                    SpO2 = r.Spo2,
                    Ax = r.AccelX,
                    Ay = r.AccelY,
                    Az = r.AccelZ,
                    Gx = r.GyroX,
                    Gy = r.GyroY,
                    Gz = r.GyroZ,
                })
                .ToList();

            dgvSensorReadings.Visible = true;
        }

        private void UpdateConnectionStatus()
        {
            if (string.IsNullOrWhiteSpace(_selectedStudentDeviceId))
            {
                lblConnectionStatus.Text = "Connection: select a student";
                lblConnectionStatus.ForeColor = Color.DimGray;
                return;
            }

            using var db = new AppDbContext();
            var lastReading = db.SensorReadings
                .Where(r => r.DeviceId == _selectedStudentDeviceId)
                .OrderByDescending(r => r.ReceivedAtUtc)
                .Select(r => (DateTime?)r.ReceivedAtUtc)
                .FirstOrDefault();

            if (lastReading is null)
            {
                lblConnectionStatus.Text = $"Connection ({_selectedStudentName}): Not connected (no readings yet)";
                lblConnectionStatus.ForeColor = Color.DarkRed;
                return;
            }

            var elapsed = DateTime.UtcNow - lastReading.Value;
            if (elapsed <= StudentOnlineWindow)
            {
                lblConnectionStatus.Text = $"Connection ({_selectedStudentName}): Connected";
                lblConnectionStatus.ForeColor = Color.DarkGreen;
            }
            else
            {
                var lastSeenLocal = lastReading.Value.ToLocalTime().ToString("g");
                lblConnectionStatus.Text = $"Connection ({_selectedStudentName}): Disconnected (last seen {lastSeenLocal})";
                lblConnectionStatus.ForeColor = Color.DarkRed;
            }
        }

        private void ResetSelectedStudentDisplay()
        {
            _selectedStudentId = null;
            _selectedStudentName = null;
            _selectedStudentDeviceId = null;

            lblSelectedStudent.Text = "Selected Student: none";
            lblDecisionStatus.Text = "Decision: select a student to view live readings";
            lblDecisionStatus.ForeColor = Color.Black;
            lblConnectionStatus.Text = "Connection: select a student";
            lblConnectionStatus.ForeColor = Color.DimGray;

            dgvSensorReadings.DataSource = null;
            dgvSensorReadings.Visible = false;
        }

        private void btnSaveProfile_Click(object sender, EventArgs e)
        {
            var fullName = txtTeacherFullName.Text.Trim();
            var phoneSuffix = txtTeacherPhoneSuffix.Text.Trim();

            if (string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show("Full name is required.", "Profile", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var db = new AppDbContext();
            var teacher = db.Users.FirstOrDefault(u => u.Id == _currentTeacher.Id && u.Role == "Teacher");
            if (teacher == null)
            {
                MessageBox.Show("Teacher profile not found.", "Profile", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            teacher.FullName = fullName;

            if (string.IsNullOrWhiteSpace(phoneSuffix))
            {
                teacher.TeacherPhoneNumber = null;
            }
            else
            {
                if (phoneSuffix.Length != 9 || !phoneSuffix.All(char.IsDigit))
                {
                    MessageBox.Show("Phone number must be 9 digits after +639.", "Profile", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                teacher.TeacherPhoneNumber = $"+639{phoneSuffix}";
            }

            db.SaveChanges();

            _currentTeacher.FullName = teacher.FullName;
            _currentTeacher.TeacherPhoneNumber = teacher.TeacherPhoneNumber;

            UpdateTeacherPhoneLabel();
            MessageBox.Show("Profile updated successfully.", "Profile", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnAddStudent_Click(object sender, EventArgs e)
        {
            var fullName = txtStudentFullName.Text.Trim();
            var ageRaw = txtStudentAge.Text.Trim();
            var sex = cmbStudentSex.SelectedItem?.ToString();

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(ageRaw) || string.IsNullOrWhiteSpace(sex))
            {
                MessageBox.Show("Fill student name, age, and sex.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(ageRaw, out var age) || age < 1 || age > 120)
            {
                MessageBox.Show("Age must be a whole number between 1 and 120.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            txtStudentFullName.Clear();
            txtStudentAge.Clear();
            cmbStudentSex.SelectedIndex = -1;

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

        private void dgvStudents_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvStudents.CurrentRow == null)
            {
                ResetSelectedStudentDisplay();
                return;
            }

            var idRaw = dgvStudents.CurrentRow.Cells["Id"]?.Value?.ToString();
            if (!int.TryParse(idRaw, out var selectedId))
            {
                ResetSelectedStudentDisplay();
                return;
            }

            using var db = new AppDbContext();
            var student = db.Users.FirstOrDefault(u => u.Id == selectedId && u.Role == "Student");
            if (student == null)
            {
                ResetSelectedStudentDisplay();
                return;
            }

            _selectedStudentId = student.Id;
            _selectedStudentName = student.FullName;
            _selectedStudentDeviceId = student.DeviceId;

            lblSelectedStudent.Text = $"Selected Student: {_selectedStudentName}";
            LoadSensorReadingsForSelectedStudent();
            UpdateConnectionStatus();

            if (!string.IsNullOrWhiteSpace(_selectedStudentDeviceId))
            {
                using var sensorDb = new AppDbContext();
                var latest = sensorDb.SensorReadings
                    .Where(r => r.DeviceId == _selectedStudentDeviceId)
                    .OrderByDescending(r => r.ReceivedAtUtc)
                    .FirstOrDefault();

                if (latest != null)
                {
                    UpdateDecisionStatus(latest);
                    return;
                }
            }

            lblDecisionStatus.Text = "Decision: waiting for sensor data...";
            lblDecisionStatus.ForeColor = Color.Black;
        }

        private void SensorListener_SensorReadingReceived(SensorReading reading)
        {
            if (!IsHandleCreated)
            {
                return;
            }

            BeginInvoke(() =>
            {
                if (_selectedStudentId == null || string.IsNullOrWhiteSpace(_selectedStudentDeviceId))
                {
                    return;
                }

                if (!string.Equals(reading.DeviceId, _selectedStudentDeviceId, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                LoadSensorReadingsForSelectedStudent();
                UpdateDecisionStatus(reading);
                UpdateConnectionStatus();
            });
        }

        private void UpdateDecisionStatus(SensorReading reading)
        {
            var alerts = new List<string>();

            if (reading.BodyTemperatureC is > 38.0) alerts.Add($"High temp {reading.BodyTemperatureC:0.0}C");
            if (reading.Spo2 is < 92.0) alerts.Add($"Low SpO2 {reading.Spo2:0.0}%");
            if (reading.HeartRate is > 120.0) alerts.Add($"High HR {reading.HeartRate:0}");

            if (alerts.Count == 0)
            {
                lblDecisionStatus.Text = $"Decision: normal ({reading.DeviceId} @ {DateTime.Now:T})";
                lblDecisionStatus.ForeColor = Color.DarkGreen;
            }
            else
            {
                lblDecisionStatus.Text = $"Decision: ALERT - {string.Join("; ", alerts)}";
                lblDecisionStatus.ForeColor = Color.DarkRed;
            }
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
                _twilioService.OpenConsole();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open Twilio console link: {ex.Message}", "Twilio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnTwilioConfigCheck_Click(object sender, EventArgs e)
        {
            var missing = _twilioService.GetMissingConfigKeys();
            var hasTeacherPhone = !string.IsNullOrWhiteSpace(_currentTeacher.TeacherPhoneNumber);

            if (missing.Count == 0 && hasTeacherPhone)
            {
                MessageBox.Show("Twilio configuration check passed. You can send a test SMS now.", "Twilio", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                "Twilio configuration is incomplete:" + Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, issues),
                "Twilio Config Check",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private void btnTwilioTest_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentTeacher.TeacherPhoneNumber))
            {
                MessageBox.Show("Teacher phone number is not set. Update your profile first.", "Twilio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var missing = _twilioService.GetMissingConfigKeys();
            if (missing.Count > 0)
            {
                MessageBox.Show("Twilio is not configured. Missing: " + string.Join(", ", missing), "Twilio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var sid = _twilioService.SendTestNotificationToTeacher(_currentTeacher.TeacherPhoneNumber, _currentTeacher.FullName);
                MessageBox.Show($"Twilio test SMS sent successfully. SID: {sid}", "Twilio", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Twilio test failed: {ex.Message}", "Twilio", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
