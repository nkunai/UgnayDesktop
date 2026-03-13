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
    private enum DashboardSeverity
    {
        NoData = 0,
        Normal = 1,
        Warning = 2,
        Critical = 3,
    }

    private enum StudentCardFilterMode
    {
        All,
        Connected,
        Disconnected,
        NeedsAttention,
    }

    private enum StudentCardSortMode
    {
        SeverityThenName,
        Name,
        ConnectionStatus,
    }

    private sealed class StudentCardView
    {
        public int StudentId { get; init; }
        public string FullName { get; set; } = string.Empty;
        public string? DeviceId { get; init; }
        public StudentCardControl CardControl { get; init; } = null!;
        public SensorReading? LatestReading { get; set; }
        public DateTime? LastReadingUtc { get; set; }
        public DashboardSeverity Severity { get; set; }
    }

    private readonly List<StudentCardView> _studentCards = new();
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
            _studentCardsPanel.BringToFront();
        }

        if (_studentCardsTimer == null)
        {
            _studentCardsTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _studentCardsTimer.Tick += (_, _) => RefreshStudentDashboardState();
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
    }

    private void RenderStudentCards(IEnumerable<(int StudentId, string FullName, string? DeviceId)> students)
    {
        InitializeStudentCardsUi();
        if (_studentCardsPanel == null)
        {
            return;
        }

        _studentCards.Clear();
        _studentCardsByDeviceId.Clear();

        foreach (var student in students.OrderBy(s => s.FullName, StringComparer.CurrentCultureIgnoreCase))
        {
            var card = BuildStudentCard(student.StudentId, student.FullName, student.DeviceId);
            _studentCards.Add(card);

            if (!string.IsNullOrWhiteSpace(student.DeviceId))
            {
                _studentCardsByDeviceId[student.DeviceId] = card;
            }
        }

        RefreshCardValuesFromDatabase();
        ApplyStudentCardFiltersAndSummary();
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
            ConnectionValue = "Status: waiting for sensor data",
            LastSeenValue = "Last reading: no data yet",
        };

        var card = new StudentCardView
        {
            StudentId = studentId,
            FullName = fullName,
            DeviceId = deviceId,
            CardControl = cardControl,
            Severity = DashboardSeverity.NoData,
        };

        cardControl.EditRequested += (_, _) => EditStudentNameFromCard(card);
        cardControl.DetailsRequested += (_, _) => ShowStudentDetails(card);
        cardControl.AlertRequested += async (_, _) => await PromptAndSendStudentAlertAsync(card);

        UpdateCardPresentation(card);
        return card;
    }

    private void RefreshCardValuesFromDatabase()
    {
        if (_studentCards.Count == 0)
        {
            return;
        }

        using var db = new AppDbContext();

        foreach (var card in _studentCards)
        {
            if (string.IsNullOrWhiteSpace(card.DeviceId))
            {
                ApplyReadingToCard(card, null);
                continue;
            }

            var latest = db.SensorReadings
                .Where(r => r.DeviceId == card.DeviceId)
                .OrderByDescending(r => r.ReceivedAtUtc)
                .ThenByDescending(r => r.Id)
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
        ApplyStudentCardFiltersAndSummary();
    }

    private void ApplyReadingToCard(StudentCardView card, SensorReading? reading)
    {
        card.LatestReading = reading;
        card.CardControl.DeviceIdValue = string.IsNullOrWhiteSpace(card.DeviceId) ? "Not assigned" : card.DeviceId;

        if (reading == null)
        {
            card.LastReadingUtc = null;
            card.Severity = DashboardSeverity.NoData;
            card.CardControl.GestureValue = "Waiting for gesture";
            card.CardControl.HeartRateValue = "--";
            card.CardControl.SweatnessValue = "--";
            card.CardControl.TemperatureValue = "--";
            UpdateCardPresentation(card);
            return;
        }

        card.LastReadingUtc = reading.ReceivedAtUtc;
        card.Severity = EvaluateSeverity(reading);
        card.CardControl.GestureValue = ResolveRecognizedGesture(reading);
        card.CardControl.HeartRateValue = reading.HeartRate.HasValue ? $"{reading.HeartRate.Value:0} bpm" : "--";
        card.CardControl.SweatnessValue = reading.GsrValue.HasValue ? $"{reading.GsrValue.Value:0.00}" : "--";
        card.CardControl.TemperatureValue = reading.BodyTemperatureC.HasValue ? $"{reading.BodyTemperatureC.Value:0.0} C" : "--";
        UpdateCardPresentation(card);
    }

    private void RefreshStudentDashboardState()
    {
        foreach (var card in _studentCards)
        {
            UpdateCardPresentation(card);
        }

        ApplyStudentCardFiltersAndSummary();
    }

    private void UpdateCardPresentation(StudentCardView card)
    {
        var severityColors = GetSeverityColors(card.Severity);
        card.CardControl.SeverityText = GetSeverityText(card.Severity);
        card.CardControl.SeverityBackColor = severityColors.BackColor;
        card.CardControl.SeverityForeColor = severityColors.ForeColor;
        card.CardControl.AccentColor = severityColors.AccentColor;

        if (card.LastReadingUtc is null)
        {
            card.CardControl.ConnectionValue = "Status: waiting for sensor data";
            card.CardControl.ConnectionValueColor = Color.DimGray;
            card.CardControl.LastSeenValue = "Last reading: no data yet";
            return;
        }

        var localLastSeen = card.LastReadingUtc.Value.ToLocalTime().ToString("g");
        card.CardControl.LastSeenValue = $"Last reading: {localLastSeen}";

        if (IsStudentConnected(card))
        {
            card.CardControl.ConnectionValue = "Status: connected";
            card.CardControl.ConnectionValueColor = Color.DarkGreen;
            return;
        }

        card.CardControl.ConnectionValue = $"Status: disconnected (last seen {localLastSeen})";
        card.CardControl.ConnectionValueColor = Color.DarkRed;
    }

    private void ApplyStudentCardFiltersAndSummary()
    {
        if (_studentCardsPanel == null)
        {
            return;
        }

        var visibleCards = GetVisibleCards().ToList();

        _studentCardsPanel.SuspendLayout();
        _studentCardsPanel.Controls.Clear();

        if (_studentCards.Count == 0)
        {
            _studentCardsPanel.Controls.Add(CreateEmptyStateCard("No students yet", "Click Add Student to create the first student card."));
        }
        else if (visibleCards.Count == 0)
        {
            _studentCardsPanel.Controls.Add(CreateEmptyStateCard("No matching students", "Try a different search term or filter to see more students."));
        }
        else
        {
            foreach (var card in visibleCards)
            {
                _studentCardsPanel.Controls.Add(card.CardControl);
            }
        }

        _studentCardsPanel.ResumeLayout();
        UpdateDashboardSummary();
    }

    private IEnumerable<StudentCardView> GetVisibleCards()
    {
        var search = NormalizeSearchText(txtStudentSearch.Text);
        var filterMode = GetSelectedFilterMode();
        var sortMode = GetSelectedSortMode();

        IEnumerable<StudentCardView> cards = _studentCards;

        if (!string.IsNullOrWhiteSpace(search))
        {
            cards = cards.Where(card =>
                NormalizeSearchText(card.FullName).Contains(search, StringComparison.OrdinalIgnoreCase) ||
                NormalizeSearchText(card.DeviceId).Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        cards = cards.Where(card => MatchesFilter(card, filterMode));

        return sortMode switch
        {
            StudentCardSortMode.Name => cards.OrderBy(card => card.FullName, StringComparer.CurrentCultureIgnoreCase),
            StudentCardSortMode.ConnectionStatus => cards
                .OrderByDescending(card => IsStudentConnected(card))
                .ThenByDescending(card => card.Severity)
                .ThenBy(card => card.FullName, StringComparer.CurrentCultureIgnoreCase),
            _ => cards
                .OrderByDescending(card => card.Severity)
                .ThenByDescending(card => IsStudentConnected(card))
                .ThenBy(card => card.FullName, StringComparer.CurrentCultureIgnoreCase),
        };
    }

    private void UpdateDashboardSummary()
    {
        var total = _studentCards.Count;
        var connected = _studentCards.Count(IsStudentConnected);
        var disconnected = total - connected;
        var attention = _studentCards.Count(card => card.Severity is DashboardSeverity.Warning or DashboardSeverity.Critical);

        lblTotalStudentsValue.Text = total.ToString();
        lblConnectedStudentsValue.Text = connected.ToString();
        lblDisconnectedStudentsValue.Text = disconnected.ToString();
        lblAttentionStudentsValue.Text = attention.ToString();
    }

    private static Panel CreateEmptyStateCard(string titleText, string subtitleText)
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
            Text = titleText,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(16, 20),
        };

        var subtitle = new Label
        {
            Text = subtitleText,
            ForeColor = Color.DimGray,
            AutoSize = false,
            Size = new Size(376, 78),
            Location = new Point(16, 56),
        };

        panel.Controls.Add(title);
        panel.Controls.Add(subtitle);
        return panel;
    }

    private bool IsStudentConnected(StudentCardView card)
    {
        return card.LastReadingUtc is not null && DateTime.UtcNow - card.LastReadingUtc.Value <= StudentOnlineWindow;
    }

    private static string NormalizeSearchText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : new string(value.Trim().ToLowerInvariant().Where(c => !char.IsWhiteSpace(c)).ToArray());
    }

    private StudentCardFilterMode GetSelectedFilterMode()
    {
        return cmbStudentFilter.SelectedIndex switch
        {
            1 => StudentCardFilterMode.Connected,
            2 => StudentCardFilterMode.Disconnected,
            3 => StudentCardFilterMode.NeedsAttention,
            _ => StudentCardFilterMode.All,
        };
    }

    private StudentCardSortMode GetSelectedSortMode()
    {
        return cmbStudentSort.SelectedIndex switch
        {
            1 => StudentCardSortMode.Name,
            2 => StudentCardSortMode.ConnectionStatus,
            _ => StudentCardSortMode.SeverityThenName,
        };
    }

    private bool MatchesFilter(StudentCardView card, StudentCardFilterMode filterMode)
    {
        return filterMode switch
        {
            StudentCardFilterMode.Connected => IsStudentConnected(card),
            StudentCardFilterMode.Disconnected => !IsStudentConnected(card),
            StudentCardFilterMode.NeedsAttention => card.Severity is DashboardSeverity.Warning or DashboardSeverity.Critical,
            _ => true,
        };
    }

    private static DashboardSeverity EvaluateSeverity(SensorReading reading)
    {
        var severity = DashboardSeverity.NoData;

        if (reading.HeartRate.HasValue)
        {
            severity = MaxSeverity(severity, reading.HeartRate.Value >= 120 ? DashboardSeverity.Critical : reading.HeartRate.Value >= 100 ? DashboardSeverity.Warning : DashboardSeverity.Normal);
        }

        if (reading.GsrValue.HasValue)
        {
            severity = MaxSeverity(severity, reading.GsrValue.Value >= 0.8 ? DashboardSeverity.Critical : reading.GsrValue.Value >= 0.5 ? DashboardSeverity.Warning : DashboardSeverity.Normal);
        }

        if (reading.BodyTemperatureC.HasValue)
        {
            severity = MaxSeverity(severity, reading.BodyTemperatureC.Value >= 38.0 ? DashboardSeverity.Critical : reading.BodyTemperatureC.Value >= 37.5 ? DashboardSeverity.Warning : DashboardSeverity.Normal);
        }

        return severity == DashboardSeverity.NoData ? DashboardSeverity.NoData : severity;
    }

    private static DashboardSeverity MaxSeverity(DashboardSeverity left, DashboardSeverity right)
    {
        return (DashboardSeverity)Math.Max((int)left, (int)right);
    }

    private static string GetSeverityText(DashboardSeverity severity)
    {
        return severity switch
        {
            DashboardSeverity.Normal => "Normal",
            DashboardSeverity.Warning => "Warning",
            DashboardSeverity.Critical => "Critical",
            _ => "No Data",
        };
    }

    private static (Color BackColor, Color ForeColor, Color AccentColor) GetSeverityColors(DashboardSeverity severity)
    {
        return severity switch
        {
            DashboardSeverity.Normal => (Color.FromArgb(223, 244, 231), Color.FromArgb(19, 109, 62), Color.FromArgb(55, 143, 93)),
            DashboardSeverity.Warning => (Color.FromArgb(255, 239, 214), Color.FromArgb(163, 98, 18), Color.FromArgb(214, 147, 44)),
            DashboardSeverity.Critical => (Color.FromArgb(253, 224, 220), Color.FromArgb(163, 49, 36), Color.FromArgb(210, 91, 77)),
            _ => (Color.FromArgb(231, 236, 241), Color.FromArgb(95, 105, 115), Color.FromArgb(155, 163, 172)),
        };
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
            ApplyStudentCardFiltersAndSummary();
            MessageBox.Show("Student name updated.", "Student", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        finally
        {
            _isEditingStudentName = false;
        }
    }

    private void ShowStudentDetails(StudentCardView card)
    {
        using var dialog = new StudentDetailsDialog(
            card.FullName,
            card.DeviceId ?? "Not assigned",
            GetSeverityText(card.Severity),
            card.CardControl.ConnectionValue,
            card.CardControl.LastSeenValue,
            card.CardControl.HeartRateValue,
            card.CardControl.SweatnessValue,
            card.CardControl.TemperatureValue,
            card.CardControl.GestureValue);
        dialog.ShowDialog(this);
    }

    private async Task PromptAndSendStudentAlertAsync(StudentCardView card)
    {
        using var dialog = new StudentAlertDialog(card.FullName, BuildAlertMessage(card));
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        await SendTeacherAlertAsync($"{card.FullName} Alert", dialog.AlertMessage);
    }

    private string BuildAlertMessage(StudentCardView card)
    {
        var connection = card.CardControl.ConnectionValue.Replace("Status: ", string.Empty, StringComparison.OrdinalIgnoreCase);
        return $"Ugnay teacher alert for {card.FullName}. Severity: {GetSeverityText(card.Severity)}. Heart rate: {card.CardControl.HeartRateValue}. Sweatness: {card.CardControl.SweatnessValue}. Temperature: {card.CardControl.TemperatureValue}. Gesture: {card.CardControl.GestureValue}. Connection: {connection}.";
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
            // ignore malformed payloads
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
