using UgnayDesktop.Controls;
using UgnayDesktop.Data;
using UgnayDesktop.Models;
using UgnayDesktop.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using BCrypt.Net;

namespace UgnayDesktop.Forms
{
    public partial class AdminDashboard : Form
    {
        private const int SensorPageSize = 80;
        private static readonly TimeSpan DeviceOnlineWindow = TimeSpan.FromSeconds(30);
        private const string ThemeLight = "Light";
        private const string ThemeDark = "Dark";

        private readonly User _currentUser;
        private readonly MqttService _mqttService = new();
        private readonly TelemetryIngestionService _telemetryIngestion = new();
        private readonly AlertDecisionService _alertDecisionService = new();
        private readonly AlertOutboxService _alertOutboxService = new();
        private readonly GestureQualityService _gestureQualityService = new();
        private readonly ToolTip _uiToolTip = new();

        private int? _selectedUserId;

        private string _sensorSearchTerm = string.Empty;
        private string? _sensorStudentDeviceFilter;
        private string _sensorGestureFilter = "All Gestures";
        private TimeSpan? _sensorWindow = TimeSpan.FromHours(6);
        private bool _sensorAlertOnly;

        private Label? _kpiConnectedStudentsLabel;
        private Label? _kpiActiveAlertsLabel;
        private Label? _kpiLastGestureLabel;
        private Label? _kpiAvgConfidenceLabel;

        private TextBox? _sensorSearchTextBox;
        private ComboBox? _sensorStudentComboBox;
        private ComboBox? _sensorGestureComboBox;
        private ComboBox? _sensorWindowComboBox;
        private CheckBox? _sensorAlertOnlyCheckBox;

        private Button? _sensorClearFiltersButton;
        private Button? _alertHistoryRefreshButton;

        private CheckBox? _themeToggleCheckBox;
        private bool _isDarkTheme;
        private bool _isThemeApplying;

        private Panel? _vitalsTrendChart;
        private Panel? _confidenceTrendChart;

        private readonly System.Windows.Forms.Timer _alertHistoryRefreshTimer = new();
        private DataGridView? _alertHistoryGrid;
        private ComboBox? _alertHistoryStatusComboBox;
        private Label? _alertHistoryCountLabel;

        public AdminDashboard(User currentUser)
        {
            _currentUser = currentUser;
            _isDarkTheme = string.Equals(_currentUser.ThemePreference, ThemeDark, StringComparison.OrdinalIgnoreCase);

            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            DoubleBuffered = true;
            ResizeRedraw = true;
            _mqttService.MessageReceived += MqttService_MessageReceived;

            CreateThemeToggleControl();
            InitializeTelemetryInsightsUi();
            InitializeAlertHistoryUi();

            ApplyModernTheme();

            LoadUsers();
            LoadSensorReadings();
            lblDecisionStatus.Text = "Decision: waiting for gesture telemetry...";
            lblSelectedUser.Text = "Selected user: none";
            UpdateProfileFieldState();
            Shown += AdminDashboard_Shown;
        }

        private void ApplyModernTheme()
        {
            _isThemeApplying = true;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1088, 760);

            var mode = _isDarkTheme ? DashboardThemeMode.Dark : DashboardThemeMode.Light;
            var palette = DashboardTheme.GetPalette(mode);

            var secondaryButtons = new List<Button> { btnLogout };
            if (_sensorClearFiltersButton != null)
            {
                secondaryButtons.Add(_sensorClearFiltersButton);
            }
            if (_alertHistoryRefreshButton != null)
            {
                secondaryButtons.Add(_alertHistoryRefreshButton);
            }

            var grids = new List<DataGridView> { dgvTeachers, dgvStudents, dgvSensorReadings };
            if (_alertHistoryGrid != null)
            {
                grids.Add(_alertHistoryGrid);
            }

            DashboardTheme.Apply(
                this,
                primaryButtons: new[] { btnAddUser, btnUpdateUser, btnMqttTest },
                secondaryButtons: secondaryButtons,
                dangerButtons: new[] { btnDeleteUser },
                grids: grids,
                mode: mode);

            labelTeachers.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold, GraphicsUnit.Point);
            labelStudents.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold, GraphicsUnit.Point);

            lblDecisionStatus.BackColor = _isDarkTheme
                ? ColorTranslator.FromHtml("#143247")
                : ColorTranslator.FromHtml("#EAF6FB");
            lblDecisionStatus.Padding = new Padding(8, 4, 8, 4);

            lblSelectedUser.BackColor = _isDarkTheme
                ? ColorTranslator.FromHtml("#1A2D44")
                : ColorTranslator.FromHtml("#EEF3F8");
            lblSelectedUser.Padding = new Padding(8, 4, 8, 4);

            txtNewPassword.UseSystemPasswordChar = true;

            lblSelectedUser.AutoSize = false;
            lblSelectedUser.AutoEllipsis = true;
            lblSelectedUser.Size = new Size(268, 40);

            lblDecisionStatus.AutoSize = false;
            lblDecisionStatus.AutoEllipsis = true;
            lblDecisionStatus.Size = new Size(474, 30);
            lblDecisionStatus.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            StyleKpiChip(_kpiConnectedStudentsLabel, palette);
            StyleKpiChip(_kpiActiveAlertsLabel, palette);
            StyleKpiChip(_kpiLastGestureLabel, palette);
            StyleKpiChip(_kpiAvgConfidenceLabel, palette);

            ApplyTrendChartTheme(_vitalsTrendChart, palette);
            ApplyTrendChartTheme(_confidenceTrendChart, palette);

            if (_themeToggleCheckBox != null)
            {
                _themeToggleCheckBox.Checked = _isDarkTheme;
                _themeToggleCheckBox.ForeColor = palette.Ink;
            }

            _isThemeApplying = false;

            ApplySensorGestureChipStyles();
            ColorAlertHistoryRows();
        }

        private async void AdminDashboard_Shown(object? sender, EventArgs e)
        {
            try
            {
                await _mqttService.SubscribeAsync("camera/data");
                await _mqttService.SubscribeAsync("camera/+/data");
                await _mqttService.SubscribeAsync("esp32/data");
                await _mqttService.SubscribeAsync("esp32/+/data");
            }
            catch (Exception ex)
            {
                AppLogger.Error("AdminDashboard", "MQTT subscribe failed.", ex);
                MessageBox.Show($"MQTT subscribe failed: {ex.Message}", "MQTT", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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

            PopulateSensorStudentFilterOptions(db);

            _selectedUserId = null;
            lblSelectedUser.Text = "Selected user: none";
            dgvTeachers.ClearSelection();
            dgvStudents.ClearSelection();
            ClearProfileInputs();
        }

        private void LoadSensorReadings()
        {
            using var db = new AppDbContext();

            var utcNow = DateTime.UtcNow;
            var filteredReadingsQuery = ApplySensorFilters(db.SensorReadings, utcNow);
            var rows = filteredReadingsQuery
                .OrderByDescending(r => r.ReceivedAtUtc)
                .Take(SensorPageSize)
                .Select(r => new
                {
                    r.DeviceId,
                    r.ReceivedAtUtc,
                    HandGesture = r.HandGesture,
                    GestureConfidence = r.HandGestureConfidence,
                    HandTracked = r.HandTracked,
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

            dgvSensorReadings.DataSource = rows;
            ApplySensorGestureChipStyles();

            var filteredForKpi = ApplySensorFilters(db.SensorReadings, utcNow);
            UpdateAdminKpis(filteredForKpi);
            UpdateAdminTrendCharts(filteredForKpi, utcNow);
            LoadAlertHistory();
        }

        private IQueryable<SensorReading> ApplySensorFilters(IQueryable<SensorReading> query, DateTime utcNow)
        {
            if (_sensorWindow is TimeSpan window)
            {
                var startUtc = utcNow - window;
                query = query.Where(r => r.ReceivedAtUtc >= startUtc);
            }

            if (!string.IsNullOrWhiteSpace(_sensorSearchTerm))
            {
                query = query.Where(r =>
                    r.DeviceId.Contains(_sensorSearchTerm) ||
                    (r.HandGesture != null && r.HandGesture.Contains(_sensorSearchTerm)));
            }

            if (!string.IsNullOrWhiteSpace(_sensorStudentDeviceFilter))
            {
                query = query.Where(r => r.DeviceId == _sensorStudentDeviceFilter);
            }

            switch (_sensorGestureFilter)
            {
                case "Open Palm":
                    query = query.Where(r => r.HandGesture != null && r.HandGesture.ToLower().Contains("open"));
                    break;
                case "Fist":
                    query = query.Where(r => r.HandGesture != null && r.HandGesture.ToLower().Contains("fist"));
                    break;
                case "No Hand":
                    query = query.Where(r => r.HandTracked == false || r.HandGesture == null || r.HandGesture == "");
                    break;
            }

            if (_sensorAlertOnly)
            {
                query = query.Where(r =>
                    r.BodyTemperatureC > 38.0 ||
                    r.Spo2 < 92.0 ||
                    r.HeartRate > 120.0);
            }

            return query;
        }

        private void InitializeTelemetryInsightsUi()
        {
            CreateKpiCards();
            CreateSensorFilterControls();
            CreateTrendCharts();
        }

        private void CreateThemeToggleControl()
        {
            _themeToggleCheckBox = chkDarkTheme;
            _themeToggleCheckBox.Checked = _isDarkTheme;
            _themeToggleCheckBox.CheckedChanged += (_, _) =>
            {
                if (_isThemeApplying)
                {
                    return;
                }

                _isDarkTheme = _themeToggleCheckBox.Checked;
                _currentUser.ThemePreference = _isDarkTheme ? ThemeDark : ThemeLight;
                SaveThemePreference();
                ApplyModernTheme();
            };

            _uiToolTip.SetToolTip(_themeToggleCheckBox, "Switch dashboard between light and dark themes.");
        }
        private void CreateKpiCards()
        {
            _kpiConnectedStudentsLabel = lblKpiConnectedStudents;
            _kpiActiveAlertsLabel = lblKpiActiveAlerts;
            _kpiLastGestureLabel = lblKpiLastGesture;
            _kpiAvgConfidenceLabel = lblKpiAvgConfidence;
        }
        private void CreateSensorFilterControls()
        {
            _sensorSearchTextBox = txtSensorSearch;
            _sensorStudentComboBox = cmbSensorStudent;
            _sensorGestureComboBox = cmbSensorGesture;
            _sensorWindowComboBox = cmbSensorWindow;
            _sensorAlertOnlyCheckBox = chkSensorAlertOnly;
            _sensorClearFiltersButton = btnSensorReset;

            _sensorSearchTextBox.TextChanged += (_, _) =>
            {
                _sensorSearchTerm = _sensorSearchTextBox.Text.Trim();
                LoadSensorReadings();
            };

            _sensorStudentComboBox.SelectedIndexChanged += (_, _) =>
            {
                _sensorStudentDeviceFilter = _sensorStudentComboBox.SelectedValue as string;
                LoadSensorReadings();
            };

            _sensorGestureComboBox.Items.Clear();
            _sensorGestureComboBox.Items.AddRange(new object[]
            {
                "All Gestures",
                "Open Palm",
                "Fist",
                "No Hand",
            });
            _sensorGestureComboBox.SelectedIndex = 0;
            _sensorGestureComboBox.SelectedIndexChanged += (_, _) =>
            {
                _sensorGestureFilter = _sensorGestureComboBox.SelectedItem?.ToString() ?? "All Gestures";
                LoadSensorReadings();
            };

            _sensorWindowComboBox.Items.Clear();
            _sensorWindowComboBox.Items.AddRange(new object[]
            {
                "30m",
                "6h",
                "24h",
                "All",
            });
            _sensorWindowComboBox.SelectedIndex = 1;
            _sensorWindowComboBox.SelectedIndexChanged += (_, _) =>
            {
                _sensorWindow = _sensorWindowComboBox.SelectedIndex switch
                {
                    0 => TimeSpan.FromMinutes(30),
                    1 => TimeSpan.FromHours(6),
                    2 => TimeSpan.FromHours(24),
                    _ => null,
                };
                LoadSensorReadings();
            };

            _sensorAlertOnlyCheckBox.CheckedChanged += (_, _) =>
            {
                _sensorAlertOnly = _sensorAlertOnlyCheckBox.Checked;
                LoadSensorReadings();
            };

            _sensorClearFiltersButton.Click += (_, _) =>
            {
                _sensorSearchTextBox.Text = string.Empty;
                _sensorStudentComboBox.SelectedIndex = 0;
                _sensorGestureComboBox.SelectedIndex = 0;
                _sensorWindowComboBox.SelectedIndex = 1;
                _sensorAlertOnlyCheckBox.Checked = false;
            };

            _uiToolTip.SetToolTip(_sensorSearchTextBox, "Search by device or gesture.");
            _uiToolTip.SetToolTip(_sensorStudentComboBox, "Filter readings by student device.");
            _uiToolTip.SetToolTip(_sensorGestureComboBox, "Filter readings by gesture type.");
            _uiToolTip.SetToolTip(_sensorWindowComboBox, "Filter readings by time range.");
            _uiToolTip.SetToolTip(_sensorAlertOnlyCheckBox, "Show only readings that trigger an alert condition.");
        }
        private void PopulateSensorStudentFilterOptions(AppDbContext db)
        {
            if (_sensorStudentComboBox == null)
            {
                return;
            }

            var options = db.Users
                .Where(u => u.Role == "Student" && u.DeviceId != null && u.DeviceId != "")
                .OrderBy(u => u.FullName)
                .Select(u => new StudentFilterOption
                {
                    Label = u.FullName,
                    DeviceId = u.DeviceId,
                })
                .ToList();

            options.Insert(0, new StudentFilterOption
            {
                Label = "All",
                DeviceId = null,
            });

            _sensorStudentComboBox.DisplayMember = nameof(StudentFilterOption.Label);
            _sensorStudentComboBox.ValueMember = nameof(StudentFilterOption.DeviceId);
            _sensorStudentComboBox.DataSource = options;

            var selected = options.FirstOrDefault(o => o.DeviceId == _sensorStudentDeviceFilter);
            _sensorStudentComboBox.SelectedItem = selected ?? options[0];
        }

        private sealed class StudentFilterOption
        {
            public string Label { get; init; } = string.Empty;
            public string? DeviceId { get; init; }
        }

        private void CreateTrendCharts()
        {
            _vitalsTrendChart = pnlVitalsTrend;
            _confidenceTrendChart = pnlConfidenceTrend;

            _vitalsTrendChart.Paint += RenderMiniTrend;
            _confidenceTrendChart.Paint += RenderMiniTrend;

            _uiToolTip.SetToolTip(_vitalsTrendChart, "Vitals trend (HR / SpO2 / Temp), last 30 minutes.");
            _uiToolTip.SetToolTip(_confidenceTrendChart, "Gesture confidence trend, last 30 minutes.");
        }
        private static Panel CreateMiniTrendChart(Point location, Size size)
        {
            var panel = new Panel
            {
                Location = location,
                Size = size,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BorderStyle = BorderStyle.FixedSingle,
            };

            panel.Paint += RenderMiniTrend;
            return panel;
        }

        private sealed record TrendSeriesData(
            string Name,
            Color Color,
            IReadOnlyList<(DateTime TimeUtc, double Value)> Points);

        private static TrendSeriesData BuildSeriesData(
            string name,
            Color color,
            IEnumerable<(DateTime TimeUtc, double? Value)> points)
        {
            var resolvedPoints = points
                .Where(p => p.Value.HasValue)
                .Select(p => (p.TimeUtc, p.Value.GetValueOrDefault()))
                .ToList();

            return new TrendSeriesData(name, color, resolvedPoints);
        }

        private static void DrawLineSeries(Panel panel, IEnumerable<TrendSeriesData> seriesData)
        {
            panel.Tag = seriesData.ToList();
            panel.Invalidate();
        }

        private static void RenderMiniTrend(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel)
            {
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(panel.BackColor);

            if (panel.Tag is not List<TrendSeriesData> seriesData || seriesData.Count == 0)
            {
                return;
            }

            var allPoints = seriesData.SelectMany(s => s.Points).ToList();
            if (allPoints.Count < 2)
            {
                return;
            }

            var minX = allPoints.Min(p => p.TimeUtc.Ticks);
            var maxX = allPoints.Max(p => p.TimeUtc.Ticks);
            var minY = allPoints.Min(p => p.Value);
            var maxY = allPoints.Max(p => p.Value);

            if (maxX == minX)
            {
                maxX = minX + 1;
            }

            if (Math.Abs(maxY - minY) < 0.0001)
            {
                maxY = minY + 1;
            }

            var rect = panel.ClientRectangle;
            const float pad = 3f;
            var width = Math.Max(1f, rect.Width - (pad * 2));
            var height = Math.Max(1f, rect.Height - (pad * 2));

            foreach (var series in seriesData)
            {
                if (series.Points.Count < 2)
                {
                    continue;
                }

                using var pen = new Pen(series.Color, 1.6f);
                var points = series.Points
                    .Select(p => new PointF(
                        pad + (float)((p.TimeUtc.Ticks - minX) * width / (maxX - minX)),
                        (pad + height) - (float)((p.Value - minY) * height / (maxY - minY))))
                    .ToArray();

                e.Graphics.DrawLines(pen, points);
            }
        }

        private void UpdateAdminTrendCharts(IQueryable<SensorReading> filteredQuery, DateTime utcNow)
        {
            if (_vitalsTrendChart == null || _confidenceTrendChart == null)
            {
                return;
            }

            var startUtc = utcNow - TimeSpan.FromMinutes(30);
            var sample = filteredQuery
                .Where(r => r.ReceivedAtUtc >= startUtc)
                .OrderBy(r => r.ReceivedAtUtc)
                .Take(360)
                .ToList();

            DrawLineSeries(
                _vitalsTrendChart,
                new[]
                {
                    BuildSeriesData("HR", ColorTranslator.FromHtml("#2563EB"), sample.Select(r => (r.ReceivedAtUtc, r.HeartRate))),
                    BuildSeriesData("SpO2", ColorTranslator.FromHtml("#16A34A"), sample.Select(r => (r.ReceivedAtUtc, r.Spo2))),
                    BuildSeriesData("Temp", ColorTranslator.FromHtml("#DC2626"), sample.Select(r => (r.ReceivedAtUtc, r.BodyTemperatureC))),
                });

            DrawLineSeries(
                _confidenceTrendChart,
                new[]
                {
                    BuildSeriesData("Confidence", ColorTranslator.FromHtml("#D97706"), sample.Select(r => (r.ReceivedAtUtc, r.HandGestureConfidence))),
                });
        }

        private static void ApplyTrendChartTheme(Panel? chart, DashboardThemePalette palette)
        {
            if (chart == null)
            {
                return;
            }

            chart.BackColor = palette.Surface;
        }

        private static void StyleKpiChip(Label? label, DashboardThemePalette palette)
        {
            if (label == null)
            {
                return;
            }

            label.BackColor = palette.SurfaceMuted;
            label.ForeColor = palette.Ink;
        }

        private void SaveThemePreference()
        {
            try
            {
                using var db = new AppDbContext();
                var user = db.Users.FirstOrDefault(u => u.Id == _currentUser.Id);
                if (user == null)
                {
                    return;
                }

                user.ThemePreference = _isDarkTheme ? ThemeDark : ThemeLight;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                AppLogger.Warning("AdminDashboard", $"Failed to save theme preference: {ex.Message}");
            }
        }

        private void InitializeAlertHistoryUi()
        {
            CreateAlertHistoryControls();
            LoadAlertHistory();

            _alertHistoryRefreshTimer.Interval = 8000;
            _alertHistoryRefreshTimer.Tick += (_, _) => LoadAlertHistory();
            _alertHistoryRefreshTimer.Start();
        }

        private void CreateAlertHistoryControls()
        {
            _alertHistoryStatusComboBox = cmbAlertHistoryStatus;
            _alertHistoryRefreshButton = btnAlertHistoryRefresh;
            _alertHistoryGrid = dgvAlertHistory;
            _alertHistoryCountLabel = lblAlertHistoryCount;

            _alertHistoryStatusComboBox.Items.Clear();
            _alertHistoryStatusComboBox.Items.AddRange(new object[]
            {
                "All Statuses",
                "Pending",
                "Processing",
                "Failed",
                "Sent",
            });
            _alertHistoryStatusComboBox.SelectedIndex = 0;
            _alertHistoryStatusComboBox.SelectedIndexChanged += (_, _) => LoadAlertHistory();

            _alertHistoryRefreshButton.Click += (_, _) => LoadAlertHistory();
            _alertHistoryGrid.SizeChanged += (_, _) => ApplyAlertHistoryColumnVisibility();

            _uiToolTip.SetToolTip(_alertHistoryStatusComboBox, "Filter alert history by delivery status.");
            _uiToolTip.SetToolTip(_alertHistoryRefreshButton, "Refresh alert history now.");
            _uiToolTip.SetToolTip(_alertHistoryGrid, "Latest alert delivery attempts from outbox.");
        }
        private void LoadAlertHistory()
        {
            if (_alertHistoryGrid == null || _alertHistoryStatusComboBox == null || _alertHistoryCountLabel == null)
            {
                return;
            }

            try
            {
                using var db = new AppDbContext();
                var query = db.AlertOutboxMessages.AsQueryable();

                var statusFilter = _alertHistoryStatusComboBox.SelectedItem?.ToString();
                if (!string.IsNullOrWhiteSpace(statusFilter)
                    && !statusFilter.StartsWith("All", StringComparison.OrdinalIgnoreCase)
                    && Enum.TryParse<AlertOutboxStatus>(statusFilter, out var parsedStatus))
                {
                    query = query.Where(item => item.Status == parsedStatus);
                }

                var rows = query
                    .OrderByDescending(item => item.CreatedAtUtc)
                    .Take(160)
                    .ToList()
                    .Select(item => new
                    {
                        Created = item.CreatedAtUtc.ToLocalTime().ToString("g"),
                        Device = item.DeviceId,
                        Severity = item.Severity,
                        Status = item.Status.ToString(),
                        Attempts = item.AttemptCount,
                        NextRetry = item.NextAttemptAtUtc.HasValue ? item.NextAttemptAtUtc.Value.ToLocalTime().ToString("g") : "-",
                        Sent = item.SentAtUtc.HasValue ? item.SentAtUtc.Value.ToLocalTime().ToString("g") : "-",
                    })
                    .ToList();

                _alertHistoryGrid.DataSource = rows;
                FormatAlertHistoryGridColumns();
                ApplyAlertHistoryColumnVisibility();
                ColorAlertHistoryRows();
                _alertHistoryCountLabel.Text = $"{rows.Count} alerts";
            }
            catch (Exception ex)
            {
                AppLogger.Warning("AdminDashboard", $"Alert history load failed: {ex.Message}");
            }
        }

        private void FormatAlertHistoryGridColumns()
        {
            if (_alertHistoryGrid == null || _alertHistoryGrid.Columns.Count == 0)
            {
                return;
            }

            var created = _alertHistoryGrid.Columns["Created"];
            var device = _alertHistoryGrid.Columns["Device"];
            var severity = _alertHistoryGrid.Columns["Severity"];
            var status = _alertHistoryGrid.Columns["Status"];
            var attempts = _alertHistoryGrid.Columns["Attempts"];
            var nextRetry = _alertHistoryGrid.Columns["NextRetry"];
            var sent = _alertHistoryGrid.Columns["Sent"];

            if (created == null || device == null || severity == null || status == null || attempts == null || nextRetry == null || sent == null)
            {
                return;
            }

            created.DisplayIndex = 0;
            device.DisplayIndex = 1;
            severity.DisplayIndex = 2;
            status.DisplayIndex = 3;
            attempts.DisplayIndex = 4;
            nextRetry.DisplayIndex = 5;
            sent.DisplayIndex = 6;

            created.Width = 74;
            device.Width = 62;
            severity.Width = 58;
            status.Width = 64;
            attempts.Width = 42;
            nextRetry.Width = 90;
            sent.Width = 78;
        }

        private void ApplyAlertHistoryColumnVisibility()
        {
            if (_alertHistoryGrid == null || _alertHistoryGrid.Columns.Count == 0)
            {
                return;
            }

            var width = _alertHistoryGrid.ClientSize.Width;
            var attempts = _alertHistoryGrid.Columns["Attempts"];
            var nextRetry = _alertHistoryGrid.Columns["NextRetry"];
            var sent = _alertHistoryGrid.Columns["Sent"];

            if (attempts == null || nextRetry == null || sent == null)
            {
                return;
            }

            attempts.Visible = width >= 250;
            sent.Visible = width >= 320;
            nextRetry.Visible = width >= 390;
        }

        private void ColorAlertHistoryRows()
        {
            if (_alertHistoryGrid == null)
            {
                return;
            }

            foreach (DataGridViewRow row in _alertHistoryGrid.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                var statusText = row.Cells["Status"]?.Value?.ToString() ?? string.Empty;
                var backColor = statusText switch
                {
                    "Pending" => ColorTranslator.FromHtml("#FFF7DB"),
                    "Processing" => ColorTranslator.FromHtml("#E6F0FF"),
                    "Failed" => ColorTranslator.FromHtml("#FFE4E6"),
                    "Sent" => ColorTranslator.FromHtml("#DCFCE7"),
                    _ => _isDarkTheme ? ColorTranslator.FromHtml("#1A2A3F") : Color.White,
                };

                row.DefaultCellStyle.BackColor = backColor;
                row.DefaultCellStyle.ForeColor = _isDarkTheme
                    ? ColorTranslator.FromHtml("#E5EDF5")
                    : ColorTranslator.FromHtml("#1E293B");
                row.DefaultCellStyle.SelectionBackColor = ControlPaint.Dark(backColor, 0.08f);
                row.DefaultCellStyle.SelectionForeColor = _isDarkTheme
                    ? Color.White
                    : ColorTranslator.FromHtml("#0F172A");
            }
        }

        private void ApplySensorGestureChipStyles()
        {
            if (dgvSensorReadings.Columns.Count == 0)
            {
                return;
            }

            var gestureColumn = dgvSensorReadings.Columns["HandGesture"];
            if (gestureColumn == null)
            {
                return;
            }

            foreach (DataGridViewRow row in dgvSensorReadings.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                var rawGesture = row.Cells["HandGesture"]?.Value?.ToString();
                var handTrackedValue = row.Cells["HandTracked"]?.Value;
                var handTracked = handTrackedValue as bool?;

                var normalized = NormalizeGestureForChip(rawGesture, handTracked);
                var chip = ResolveGestureChip(normalized);

                row.Cells["HandGesture"].Value = chip.Text;
                row.Cells["HandGesture"].Style.BackColor = chip.BackColor;
                row.Cells["HandGesture"].Style.ForeColor = chip.ForeColor;
                row.Cells["HandGesture"].Style.SelectionBackColor = ControlPaint.Dark(chip.BackColor, 0.06f);
                row.Cells["HandGesture"].Style.SelectionForeColor = chip.ForeColor;
            }
        }

        private static string NormalizeGestureForChip(string? gesture, bool? handTracked)
        {
            if (handTracked == false)
            {
                return "nohand";
            }

            if (string.IsNullOrWhiteSpace(gesture))
            {
                return "nohand";
            }

            var normalized = gesture.Trim().ToLowerInvariant();
            if (normalized.Contains("open"))
            {
                return "open";
            }

            if (normalized.Contains("fist"))
            {
                return "fist";
            }

            return normalized;
        }

        private static (string Text, Color BackColor, Color ForeColor) ResolveGestureChip(string normalizedGesture)
        {
            return normalizedGesture switch
            {
                "open" => (
                    "Open Palm",
                    ColorTranslator.FromHtml("#DCFCE7"),
                    ColorTranslator.FromHtml("#166534")),
                "fist" => (
                    "Fist",
                    ColorTranslator.FromHtml("#FFEDD5"),
                    ColorTranslator.FromHtml("#9A3412")),
                "nohand" => (
                    "No Hand",
                    ColorTranslator.FromHtml("#E2E8F0"),
                    ColorTranslator.FromHtml("#334155")),
                _ => (
                    normalizedGesture,
                    ColorTranslator.FromHtml("#E0F2FE"),
                    ColorTranslator.FromHtml("#1E3A8A")),
            };
        }

        private void UpdateAdminKpis(IQueryable<SensorReading> filteredQuery)
        {
            if (_kpiConnectedStudentsLabel == null ||
                _kpiActiveAlertsLabel == null ||
                _kpiLastGestureLabel == null ||
                _kpiAvgConfidenceLabel == null)
            {
                return;
            }

            var nowUtc = DateTime.UtcNow;
            var sample = filteredQuery
                .OrderByDescending(r => r.ReceivedAtUtc)
                .Take(300)
                .ToList();

            var connectedStudents = sample
                .Where(r => nowUtc - r.ReceivedAtUtc <= DeviceOnlineWindow)
                .Select(r => r.DeviceId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            var latestPerDevice = sample
                .GroupBy(r => r.DeviceId, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.OrderByDescending(r => r.ReceivedAtUtc).First());
            var activeAlerts = latestPerDevice.Count(IsAlertCondition);

            var latestGesture = sample
                .FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.HandGesture));

            var avgConfidence = sample
                .Where(r => r.HandGestureConfidence.HasValue)
                .Select(r => r.HandGestureConfidence!.Value)
                .DefaultIfEmpty()
                .Average();

            _kpiConnectedStudentsLabel.Text = $"Connected students: {connectedStudents}";
            _kpiActiveAlertsLabel.Text = $"Active alerts: {activeAlerts}";
            _kpiLastGestureLabel.Text = latestGesture == null
                ? "Last gesture: -"
                : $"Last gesture: {latestGesture.HandGesture}";
            _kpiAvgConfidenceLabel.Text = avgConfidence <= 0
                ? "Avg conf: -"
                : $"Avg conf: {avgConfidence:0.00}";
        }

        private static bool IsAlertCondition(SensorReading reading)
        {
            return reading.BodyTemperatureC is > 38.0
                || reading.Spo2 is < 92.0
                || reading.HeartRate is > 120.0;
        }

        private static Label CreateKpiChip(string text, int width)
        {
            return new Label
            {
                Text = text,
                Width = width,
                Height = 32,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 0, 6, 0),
                Padding = new Padding(8, 0, 8, 0),
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point),
                BorderStyle = BorderStyle.FixedSingle,
            };
        }

        private void MqttService_MessageReceived(string topic, string payload)
        {
            if (!_telemetryIngestion.IsTelemetryTopic(topic))
            {
                return;
            }

            SensorReading reading;
            try
            {
                reading = _telemetryIngestion.IngestPayload(payload);
            }
            catch (Exception ex)
            {
                AppLogger.Warning("AdminDashboard", "Invalid telemetry payload.", ex, eventName: "mqtt_payload_invalid", context: new { topic, PayloadLength = payload.Length });
                BeginInvoke(() => lblDecisionStatus.Text = $"Decision: invalid payload ({ex.Message})");
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
            var gestureQuality = _gestureQualityService.Evaluate(reading);
            var gestureStatus = gestureQuality.GestureStatusText;

            var decision = _alertDecisionService.Evaluate(reading, DateTime.UtcNow);
            TryQueueAlertOutbox(reading, decision);
            LoadAlertHistory();
            if (decision.Severity == AlertSeverity.Normal)
            {
                lblDecisionStatus.Text = $"Decision: normal ({reading.DeviceId}, {gestureStatus} @ {DateTime.Now:T})";
                lblDecisionStatus.ForeColor = Color.DarkGreen;
                return;
            }

            var severityText = decision.Severity switch
            {
                AlertSeverity.Critical => "CRITICAL",
                AlertSeverity.Warning => "WARNING",
                AlertSeverity.Info => "INFO",
                _ => "ALERT"
            };

            var color = decision.Severity switch
            {
                AlertSeverity.Critical => Color.DarkRed,
                AlertSeverity.Warning => Color.DarkOrange,
                AlertSeverity.Info => Color.SteelBlue,
                _ => Color.DarkRed,
            };

            var detailText = string.Join("; ", decision.Messages);
            if (decision.IsSuppressed && decision.CooldownUntilUtc is DateTime cooldownUntilUtc)
            {
                lblDecisionStatus.Text =
                    $"Decision: {severityText} (cooldown until {cooldownUntilUtc.ToLocalTime():T}) - {detailText} ({reading.DeviceId}, {gestureStatus} @ {DateTime.Now:T})";
            }
            else
            {
                lblDecisionStatus.Text =
                    $"Decision: {severityText} - {detailText} ({reading.DeviceId}, {gestureStatus} @ {DateTime.Now:T})";
            }

            lblDecisionStatus.ForeColor = color;
        }
        
        private void TryQueueAlertOutbox(SensorReading reading, AlertDecision decision)
        {
            try
            {
                if (_alertOutboxService.TryEnqueueFromDecision(reading, decision, nameof(AdminDashboard), out var queuedMessage)
                    && queuedMessage != null)
                {
                    AppLogger.Info("AdminDashboard", "Alert queued for delivery.", eventName: "alert_outbox_queued", context: new { queuedMessage.Id, queuedMessage.Severity, queuedMessage.DeviceId });
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("AdminDashboard", "Failed to queue alert into outbox.", ex);
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

        private async void btnMqttTest_Click(object sender, EventArgs e)
        {
            btnMqttTest.Enabled = false;

            try
            {
                var ok = await _mqttService.RunLoopbackTestAsync();

                if (ok)
                {
                    MessageBox.Show("MQTT test passed. Connected and received loopback message from localhost:1883.", "MQTT", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("MQTT test failed (timeout). Check if Mosquitto is running on localhost:1883.", "MQTT", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"MQTT test error: {ex.Message}", "MQTT", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnMqttTest.Enabled = true;
            }
        }

        private async void btnLogout_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show("Are you sure you want to log out?", "Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                _alertHistoryRefreshTimer.Stop();
                _mqttService.MessageReceived -= MqttService_MessageReceived;
                await _mqttService.DisconnectAsync();
                Close();
            }
        }
    }
}
















































