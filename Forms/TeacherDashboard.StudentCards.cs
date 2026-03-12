using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using UgnayDesktop.Controls;
using UgnayDesktop.Data;
using UgnayDesktop.Models;

namespace UgnayDesktop.Forms;

public partial class TeacherDashboard
{
    private sealed class StudentCardView
    {
        public int StudentId { get; init; }
        public string FullName { get; set; } = string.Empty;
        public string? DeviceId { get; init; }
        public StudentCardControl CardControl { get; init; } = null!;
        public DateTime? LastReadingUtc { get; set; }
    }

    private readonly Dictionary<string, StudentCardView> _studentCardsByDeviceId = new(StringComparer.OrdinalIgnoreCase);
    private FlowLayoutPanel? _studentCardsPanel;
    private System.Windows.Forms.Timer? _studentCardsTimer;
    private bool _studentCardsCleanupRegistered;
    private bool _isEditingStudentName;

    private void InitializeStudentCardsUi()
    {
        if (_studentCardsPanel == null)
        {
            _studentCardsPanel = flpStudentCards;

            if (_studentCardsPanel == null)
            {
                _studentCardsPanel = new FlowLayoutPanel
                {
                    Name = "flpStudentCards",
                    AutoScroll = true,
                    WrapContents = true,
                    FlowDirection = FlowDirection.LeftToRight,
                    BackColor = Color.FromArgb(245, 247, 250),
                    Padding = new Padding(10),
                    Margin = new Padding(0),
                    Visible = true,
                };

                Controls.Add(_studentCardsPanel);
            }

            _studentCardsPanel.BringToFront();
        }

        if (_studentCardsTimer == null)
        {
            _studentCardsTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _studentCardsTimer.Tick += (_, _) => RefreshCardConnectivityStatus();
            _studentCardsTimer.Start();
        }

        if (!_studentCardsCleanupRegistered)
        {
            FormClosed += (_, _) =>
            {
                _studentCardsTimer?.Stop();
                _studentCardsTimer?.Dispose();
                _studentCardsTimer = null;
            };

            _studentCardsCleanupRegistered = true;
        }

        SyncStudentCardsPanelBounds();
    }

    private void SyncStudentCardsPanelBounds()
    {
        if (_studentCardsPanel == null)
        {
            return;
        }

        _studentCardsPanel.BringToFront();
    }

    private void RenderStudentCards(IEnumerable<(int StudentId, string FullName, string? DeviceId)> students)
    {
        InitializeStudentCardsUi();

        if (_studentCardsPanel == null)
        {
            return;
        }

        var list = students.ToList();

        _studentCardsByDeviceId.Clear();

        _studentCardsPanel.SuspendLayout();
        _studentCardsPanel.Controls.Clear();

        if (list.Count == 0)
        {
            _studentCardsPanel.Controls.Add(CreateEmptyStateCard());
            _studentCardsPanel.ResumeLayout();
            return;
        }

        foreach (var student in list)
        {
            var card = BuildStudentCard(student.StudentId, student.FullName, student.DeviceId);
            _studentCardsPanel.Controls.Add(card.CardControl);

            if (!string.IsNullOrWhiteSpace(student.DeviceId))
            {
                _studentCardsByDeviceId[student.DeviceId] = card;
            }
        }

        _studentCardsPanel.ResumeLayout();
        RefreshCardValuesFromDatabase();
    }

    private Panel CreateEmptyStateCard()
    {
        var panel = new Panel
        {
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Width = 420,
            Height = 180,
            Margin = new Padding(12),
        };

        var title = new Label
        {
            Text = "No students yet",
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(16, 20),
        };

        var subtitle = new Label
        {
            Text = "Click Add Student to create the first student card.",
            ForeColor = Color.DimGray,
            AutoSize = false,
            Size = new Size(376, 64),
            Location = new Point(16, 56),
        };

        panel.Controls.Add(title);
        panel.Controls.Add(subtitle);
        return panel;
    }

    private StudentCardView BuildStudentCard(int studentId, string fullName, string? deviceId)
    {
        var cardControl = new StudentCardControl
        {
            Name = $"studentCard_{studentId}",
            Margin = new Padding(12),
            StudentName = fullName,
            DeviceIdValue = deviceId ?? "Not assigned",
            GestureValue = "Waiting for connection",
            HeartRateValue = "--",
            SweatnessValue = "--",
            TemperatureValue = "--",
            ConnectionValue = "No recent data",
        };

        var card = new StudentCardView
        {
            StudentId = studentId,
            FullName = fullName,
            DeviceId = deviceId,
            CardControl = cardControl,
        };

        cardControl.CardClicked += (_, _) => EditStudentNameFromCard(card);

        return card;
    }

    private void EditStudentNameFromCard(StudentCardView card)
    {
        if (_isEditingStudentName)
        {
            return;
        }

        _isEditingStudentName = true;
        try
        {
            using var dialog = new EditStudentNameDialog(card.FullName);
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var updatedName = dialog.StudentFullName;
            if (string.Equals(updatedName, card.FullName, StringComparison.Ordinal))
            {
                return;
            }

            using var db = new AppDbContext();
            var student = db.Users.FirstOrDefault(u => u.Id == card.StudentId && u.Role == "Student");
            if (student == null)
            {
                MessageBox.Show("Student record was not found.", "Student", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            student.FullName = updatedName;
            db.SaveChanges();

            card.FullName = student.FullName;
            card.CardControl.StudentName = student.FullName;

            LoadStudents();
            MessageBox.Show("Student name updated.", "Student", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        finally
        {
            _isEditingStudentName = false;
        }
    }

    private void RefreshCardValuesFromDatabase()
    {
        if (_studentCardsByDeviceId.Count == 0)
        {
            return;
        }

        using var db = new AppDbContext();

        foreach (var card in _studentCardsByDeviceId.Values)
        {
            if (string.IsNullOrWhiteSpace(card.DeviceId))
            {
                ApplyReadingToCard(card, null);
                continue;
            }

            var latest = db.SensorReadings
                .Where(r => r.DeviceId == card.DeviceId)
                .OrderByDescending(r => r.ReceivedAtUtc)
                .FirstOrDefault();

            ApplyReadingToCard(card, latest);
        }
    }

    private void UpdateStudentCardFromReading(SensorReading reading)
    {
        if (string.IsNullOrWhiteSpace(reading.DeviceId))
        {
            return;
        }

        if (!_studentCardsByDeviceId.TryGetValue(reading.DeviceId, out var card))
        {
            return;
        }

        ApplyReadingToCard(card, reading);
    }

    private void ApplyReadingToCard(StudentCardView card, SensorReading? reading)
    {
        card.CardControl.DeviceIdValue = string.IsNullOrWhiteSpace(card.DeviceId) ? "Not assigned" : card.DeviceId;

        if (reading == null)
        {
            card.LastReadingUtc = null;
            card.CardControl.GestureValue = "Waiting for connection";
            card.CardControl.HeartRateValue = "--";
            card.CardControl.SweatnessValue = "--";
            card.CardControl.TemperatureValue = "--";
            UpdateCardConnectionLabel(card);
            return;
        }

        card.LastReadingUtc = reading.ReceivedAtUtc;
        card.CardControl.GestureValue = ResolveRecognizedGesture(reading);
        card.CardControl.HeartRateValue = reading.HeartRate.HasValue ? $"{reading.HeartRate.Value:0} bpm" : "--";
        card.CardControl.SweatnessValue = reading.GsrValue.HasValue ? $"{reading.GsrValue.Value:0.00}" : "--";
        card.CardControl.TemperatureValue = reading.BodyTemperatureC.HasValue ? $"{reading.BodyTemperatureC.Value:0.0} \u00B0C" : "--";

        UpdateCardConnectionLabel(card);
    }

    private void RefreshCardConnectivityStatus()
    {
        foreach (var card in _studentCardsByDeviceId.Values)
        {
            UpdateCardConnectionLabel(card);
        }
    }

    private void UpdateCardConnectionLabel(StudentCardView card)
    {
        if (card.LastReadingUtc is null)
        {
            card.CardControl.ConnectionValue = "Status: waiting for sensor data";
            card.CardControl.ConnectionValueColor = Color.DimGray;
            return;
        }

        var elapsed = DateTime.UtcNow - card.LastReadingUtc.Value;
        if (elapsed <= StudentOnlineWindow)
        {
            card.CardControl.ConnectionValue = "Status: connected";
            card.CardControl.ConnectionValueColor = Color.DarkGreen;
            return;
        }

        var lastSeenLocal = card.LastReadingUtc.Value.ToLocalTime().ToString("g");
        card.CardControl.ConnectionValue = $"Status: disconnected (last seen {lastSeenLocal})";
        card.CardControl.ConnectionValueColor = Color.DarkRed;
    }

    private static string ResolveRecognizedGesture(SensorReading reading)
    {
        if (string.IsNullOrWhiteSpace(reading.RawJson))
        {
            return "Waiting for gesture";
        }

        try
        {
            using var doc = JsonDocument.Parse(reading.RawJson);
            var keys = new[]
            {
                "recognizedGesture",
                "recognized_gesture",
                "gesture",
                "predictedLabel",
                "prediction",
                "label"
            };

            foreach (var key in keys)
            {
                var value = TryFindJsonValueByKey(doc.RootElement, key);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
        }
        catch (JsonException)
        {
            // fall through
        }

        return "Waiting for gesture";
    }

    private static string? TryFindJsonValueByKey(JsonElement element, string key)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in element.EnumerateObject())
            {
                if (string.Equals(prop.Name, key, StringComparison.OrdinalIgnoreCase))
                {
                    return prop.Value.ValueKind == JsonValueKind.String
                        ? prop.Value.GetString()
                        : prop.Value.ToString();
                }

                var nested = TryFindJsonValueByKey(prop.Value, key);
                if (!string.IsNullOrWhiteSpace(nested))
                {
                    return nested;
                }
            }
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var nested = TryFindJsonValueByKey(item, key);
                if (!string.IsNullOrWhiteSpace(nested))
                {
                    return nested;
                }
            }
        }

        return null;
    }
}
