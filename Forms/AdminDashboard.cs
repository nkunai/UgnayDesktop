using UgnayDesktop.Data;
using UgnayDesktop.Models;
using UgnayDesktop.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BCrypt.Net;

namespace UgnayDesktop.Forms
{
    public partial class AdminDashboard : Form
    {
        private readonly User _currentUser;
        private int? _selectedUserId;

        public AdminDashboard(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            UdpSensorListener.Shared.SensorReadingReceived += SensorListener_SensorReadingReceived;
            LoadUsers();
            LoadSensorReadings();
            lblDecisionStatus.Text = "Decision: waiting for sensor data...";
            lblSelectedUser.Text = "Selected user: none";
            UpdateProfileFieldState();
            btnMqttTest.Enabled = false;
            btnMqttTest.Text = "UDP 5005 Active";
        }

        private void LoadUsers()
        {
            using var db = new AppDbContext();

            dgvTeachers.DataSource = db.Users
                .Where(u => u.Role == "Teacher")
                .OrderBy(u => u.FullName)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Username,
                    Phone = u.TeacherPhoneNumber ?? string.Empty
                })
                .ToList();

            dgvStudents.DataSource = db.Users
                .Where(u => u.Role == "Student")
                .OrderBy(u => u.FullName)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Username,
                    u.Age,
                    u.Sex,
                    Device = u.DeviceId ?? string.Empty
                })
                .ToList();

            _selectedUserId = null;
            lblSelectedUser.Text = "Selected user: none";
            dgvTeachers.ClearSelection();
            dgvStudents.ClearSelection();
            ClearProfileInputs();
        }

        private void LoadSensorReadings()
        {
            using var db = new AppDbContext();

            dgvSensorReadings.DataSource = db.SensorReadings
                .OrderByDescending(r => r.ReceivedAtUtc)
                .Take(30)
                .Select(r => new
                {
                    r.DeviceId,
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
        }

        private void SensorListener_SensorReadingReceived(SensorReading reading)
        {
            if (!IsHandleCreated)
            {
                return;
            }

            BeginInvoke(() =>
            {
                LoadSensorReadings();
                UpdateDecisionStatus(reading);
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

        private void btnAddUser_Click(object sender, EventArgs e)
        {
            if (!ValidateBaseProfileFields(requirePassword: true, out var fullName, out var username, out var role, out var password)) return;

            using var db = new AppDbContext();

            if (db.Users.Any(u => u.Username == username))
            {
                MessageBox.Show("Username already exists.");
                return;
            }

            var user = new User
            {
                FullName = fullName,
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password!),
                Role = role,
            };

            if (role == "Teacher")
            {
                var teacherPhone = txtTeacherPhone.Text.Trim();
                if (string.IsNullOrWhiteSpace(teacherPhone))
                {
                    MessageBox.Show("Teacher phone number is required.");
                    return;
                }

                user.TeacherPhoneNumber = teacherPhone;
            }

            if (role == "Student")
            {
                if (!TryReadStudentProfileFields(db, null, out var age, out var sex, out var deviceId)) return;
                user.Age = age;
                user.Sex = sex;
                user.DeviceId = deviceId;
            }

            db.Users.Add(user);
            db.SaveChanges();
            LoadUsers();
        }

        private void btnUpdateUser_Click(object sender, EventArgs e)
        {
            if (_selectedUserId == null)
            {
                MessageBox.Show("Select a teacher or student to update.");
                return;
            }

            if (!ValidateBaseProfileFields(requirePassword: false, out var fullName, out var username, out var role, out var password)) return;

            using var db = new AppDbContext();
            var user = db.Users.FirstOrDefault(u => u.Id == _selectedUserId.Value);
            if (user == null)
            {
                MessageBox.Show("User not found.");
                LoadUsers();
                return;
            }

            if (db.Users.Any(u => u.Id != user.Id && u.Username == username))
            {
                MessageBox.Show("Username already exists.");
                return;
            }

            user.FullName = fullName;
            user.Username = username;
            user.Role = role;

            if (!string.IsNullOrWhiteSpace(password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            }

            if (role == "Teacher")
            {
                var teacherPhone = txtTeacherPhone.Text.Trim();
                if (string.IsNullOrWhiteSpace(teacherPhone))
                {
                    MessageBox.Show("Teacher phone number is required.");
                    return;
                }

                user.TeacherPhoneNumber = teacherPhone;
                user.Age = null;
                user.Sex = null;
                user.DeviceId = null;
            }
            else if (role == "Student")
            {
                if (!TryReadStudentProfileFields(db, user.Id, out var age, out var sex, out var deviceId)) return;
                user.Age = age;
                user.Sex = sex;
                user.DeviceId = deviceId;
                user.TeacherPhoneNumber = null;
            }
            else
            {
                user.Age = null;
                user.Sex = null;
                user.DeviceId = null;
                user.TeacherPhoneNumber = null;
            }

            db.SaveChanges();
            LoadUsers();
            MessageBox.Show("Profile updated.");
        }

        private bool ValidateBaseProfileFields(bool requirePassword, out string fullName, out string username, out string role, out string? password)
        {
            fullName = txtFullName.Text.Trim();
            username = txtNewUsername.Text.Trim();
            role = cmbRole.SelectedItem?.ToString() ?? string.Empty;
            password = txtNewPassword.Text;

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(role))
            {
                MessageBox.Show("Fill full name, username, and role.");
                return false;
            }

            if (requirePassword && string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Password is required for new users.");
                return false;
            }

            return true;
        }

        private bool TryReadStudentProfileFields(AppDbContext db, int? currentUserId, out int age, out string sex, out string deviceId)
        {
            age = 0;
            sex = string.Empty;
            deviceId = txtStudentDeviceId.Text.Trim();

            if (!int.TryParse(txtStudentAge.Text.Trim(), out age) || age < 1 || age > 120)
            {
                MessageBox.Show("Student age must be a whole number between 1 and 120.");
                return false;
            }

            sex = cmbStudentSex.SelectedItem?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(sex))
            {
                MessageBox.Show("Select student sex.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(deviceId))
            {
                MessageBox.Show("Student device ID is required.");
                return false;
            }

            var requestedDeviceId = deviceId;
            if (db.Users.Any(u => u.Id != currentUserId && u.DeviceId == requestedDeviceId))
            {
                MessageBox.Show("Student device ID is already assigned.");
                return false;
            }

            return true;
        }

        private void btnDeleteUser_Click(object sender, EventArgs e)
        {
            if (_selectedUserId == null)
            {
                MessageBox.Show("Select a teacher or student to delete.");
                return;
            }

            var selectedUserId = _selectedUserId.Value;
            if (selectedUserId == _currentUser.Id)
            {
                MessageBox.Show("You cannot delete your own account while logged in.");
                return;
            }

            var confirm = MessageBox.Show("Are you sure you want to delete this user?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            using var db = new AppDbContext();
            var user = db.Users.FirstOrDefault(u => u.Id == selectedUserId);
            if (user == null)
            {
                MessageBox.Show("User not found.");
                LoadUsers();
                return;
            }

            db.Users.Remove(user);
            db.SaveChanges();
            LoadUsers();
        }

        private void dgvTeachers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvTeachers.CurrentRow == null) return;
            var idValue = dgvTeachers.CurrentRow.Cells["Id"]?.Value;
            if (idValue == null || !int.TryParse(idValue.ToString(), out var selectedUserId)) return;

            using var db = new AppDbContext();
            var user = db.Users.FirstOrDefault(u => u.Id == selectedUserId);
            if (user == null) return;

            _selectedUserId = selectedUserId;
            lblSelectedUser.Text = $"Selected user: {user.FullName} ({user.Role})";
            dgvStudents.ClearSelection();
            PopulateProfileFields(user);
        }

        private void dgvStudents_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvStudents.CurrentRow == null) return;
            var idValue = dgvStudents.CurrentRow.Cells["Id"]?.Value;
            if (idValue == null || !int.TryParse(idValue.ToString(), out var selectedUserId)) return;

            using var db = new AppDbContext();
            var user = db.Users.FirstOrDefault(u => u.Id == selectedUserId);
            if (user == null) return;

            _selectedUserId = selectedUserId;
            lblSelectedUser.Text = $"Selected user: {user.FullName} ({user.Role})";
            dgvTeachers.ClearSelection();
            PopulateProfileFields(user);
        }

        private void PopulateProfileFields(User user)
        {
            txtFullName.Text = user.FullName;
            txtNewUsername.Text = user.Username;
            txtNewPassword.Clear();
            cmbRole.SelectedItem = user.Role;
            txtTeacherPhone.Text = user.TeacherPhoneNumber ?? string.Empty;
            txtStudentAge.Text = user.Age?.ToString() ?? string.Empty;
            cmbStudentSex.SelectedItem = user.Sex;
            txtStudentDeviceId.Text = user.DeviceId ?? string.Empty;
            UpdateProfileFieldState();
        }

        private void ClearProfileInputs()
        {
            txtFullName.Clear();
            txtNewUsername.Clear();
            txtNewPassword.Clear();
            cmbRole.SelectedIndex = -1;
            txtTeacherPhone.Clear();
            txtStudentAge.Clear();
            cmbStudentSex.SelectedIndex = -1;
            txtStudentDeviceId.Clear();
            UpdateProfileFieldState();
        }

        private void cmbRole_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateProfileFieldState();
        }

        private void UpdateProfileFieldState()
        {
            var selectedRole = cmbRole.SelectedItem?.ToString();
            var isTeacher = string.Equals(selectedRole, "Teacher", StringComparison.OrdinalIgnoreCase);
            var isStudent = string.Equals(selectedRole, "Student", StringComparison.OrdinalIgnoreCase);

            lblTeacherPhone.Enabled = isTeacher;
            txtTeacherPhone.Enabled = isTeacher;

            lblStudentAge.Enabled = isStudent;
            txtStudentAge.Enabled = isStudent;
            lblStudentSex.Enabled = isStudent;
            cmbStudentSex.Enabled = isStudent;
            lblStudentDeviceId.Enabled = isStudent;
            txtStudentDeviceId.Enabled = isStudent;

            if (!isTeacher)
            {
                txtTeacherPhone.Clear();
            }

            if (!isStudent)
            {
                txtStudentAge.Clear();
                cmbStudentSex.SelectedIndex = -1;
                txtStudentDeviceId.Clear();
            }
        }

        private void btnMqttTest_Click(object sender, EventArgs e)
        {
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
    }
}



