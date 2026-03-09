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

namespace UgnayDesktop.Forms
{
    public partial class TeacherDashboard : Form
    {
        private const int SensorPageSize = 80;
        private static readonly TimeSpan StudentOnlineWindow = TimeSpan.FromSeconds(30);
        private const string ThemeLight = "Light";
        private const string ThemeDark = "Dark";

        private readonly MqttService _mqttService = new();
        private readonly TwilioService _twilioService = new();
        private readonly TelemetryIngestionService _telemetryIngestion = new();
        private readonly AlertDecisionService _alertDecisionService = new();
        private readonly AlertOutboxService _alertOutboxService = new();
        private readonly GestureQualityService _gestureQualityService = new();
        private readonly ToolTip _uiToolTip = new();
        private readonly User _currentTeacher;

        private int? _selectedStudentId;
        private string? _selectedStudentName;
        private string? _selectedStudentDeviceId;

        private string _studentSensorSearchTerm = string.Empty;
        private string _studentGestureFilter = "All Gestures";
        private TimeSpan? _studentSensorWindow = TimeSpan.FromHours(6);
        private bool _studentSensorAlertOnly;

        private Label? _kpiStudentReadingsLabel;
        private Label? _kpiLatestGestureLabel;
        private Label? _kpiGestureQualityLabel;
        private Label? _kpiStudentAlertsLabel;

        private TextBox? _studentSensorSearchTextBox;
        private ComboBox? _studentGestureComboBox;
        private ComboBox? _studentSensorWindowComboBox;
        private CheckBox? _studentSensorAlertOnlyCheckBox;

        private Button? _studentFilterClearButton;
        private Button? _alertHistoryRefreshButton;

        private CheckBox? _themeToggleCheckBox;
        private bool _isDarkTheme;
        private bool _isThemeApplying;

        private Panel? _studentVitalsTrendChart;
        private Panel? _studentConfidenceTrendChart;

        private readonly System.Windows.Forms.Timer _alertHistoryRefreshTimer = new();
        private DataGridView? _alertHistoryGrid;
        private ComboBox? _alertHistoryStatusComboBox;
        private Label? _alertHistoryCountLabel;

        public TeacherDashboard(User currentTeacher)
        {
            _currentTeacher = currentTeacher;
            _isDarkTheme = string.Equals(_currentTeacher.ThemePreference, ThemeDark, StringComparison.OrdinalIgnoreCase);

            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            DoubleBuffered = true;
            ResizeRedraw = true;
            _mqttService.MessageReceived += MqttService_MessageReceived;

            CreateThemeToggleControl();
            InitializeTelemetryInsightsUi();
            InitializeAlertHistoryUi();

            ApplyModernTheme();

            LoadCurrentTeacherProfile();
            LoadStudents();
            ResetSelectedStudentDisplay();
            UpdateTeacherPhoneLabel();
            Shown += TeacherDashboard_Shown;
        }

        private void ApplyModernTheme()
        {
            _isThemeApplying = true;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1184, 760);

            var mode = _isDarkTheme ? DashboardThemeMode.Dark : DashboardThemeMode.Light;
            var palette = DashboardTheme.GetPalette(mode);

            var secondaryButtons = new List<Button> { btnTwilioLink, btnLogout };
            if (_studentFilterClearButton != null)
            {
                secondaryButtons.Add(_studentFilterClearButton);
            }
            if (_alertHistoryRefreshButton != null)
            {
                secondaryButtons.Add(_alertHistoryRefreshButton);
            }

            var grids = new List<DataGridView> { dgvStudents, dgvSensorReadings };
            if (_alertHistoryGrid != null)
            {
                grids.Add(_alertHistoryGrid);
            }

            DashboardTheme.Apply(
                this,
                primaryButtons: new[] { btnSaveProfile, btnAddStudent, btnTwilioConfigCheck, btnTwilioTest },
                secondaryButtons: secondaryButtons,
                grids: grids,
                mode: mode);

            lblProfileHeader.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point);
            lblStudentHeader.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point);

            lblDecisionStatus.BackColor = _isDarkTheme
                ? ColorTranslator.FromHtml("#143247")
                : ColorTranslator.FromHtml("#EAF6FB");
            lblDecisionStatus.Padding = new Padding(8, 4, 8, 4);

            lblConnectionStatus.BackColor = _isDarkTheme
                ? ColorTranslator.FromHtml("#1A2D44")
                : ColorTranslator.FromHtml("#EEF3F8");
            lblConnectionStatus.Padding = new Padding(8, 4, 8, 4);

            lblSelectedStudent.BackColor = _isDarkTheme
                ? ColorTranslator.FromHtml("#1A2D44")
                : ColorTranslator.FromHtml("#EEF3F8");
            lblSelectedStudent.Padding = new Padding(8, 4, 8, 4);

            lblSelectedStudent.AutoSize = false;
            lblSelectedStudent.AutoEllipsis = true;
            lblSelectedStudent.Size = new Size(760, 30);
            lblSelectedStudent.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            lblConnectionStatus.AutoSize = false;
            lblConnectionStatus.AutoEllipsis = true;
            lblConnectionStatus.Size = new Size(760, 30);
            lblConnectionStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            lblDecisionStatus.AutoSize = false;
            lblDecisionStatus.AutoEllipsis = true;
            lblDecisionStatus.Size = new Size(760, 30);
            lblDecisionStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            StyleKpiChip(_kpiStudentReadingsLabel, palette);
            StyleKpiChip(_kpiLatestGestureLabel, palette);
            StyleKpiChip(_kpiGestureQualityLabel, palette);
            StyleKpiChip(_kpiStudentAlertsLabel, palette);

            ApplyTrendChartTheme(_studentVitalsTrendChart, palette);
            ApplyTrendChartTheme(_studentConfidenceTrendChart, palette);

            if (_themeToggleCheckBox != null)
            {
                _themeToggleCheckBox.Checked = _isDarkTheme;
                _themeToggleCheckBox.ForeColor = palette.Ink;
            }

            _isThemeApplying = false;

            ApplySensorGestureChipStyles();
            ColorAlertHistoryRows();
        }

        private async void TeacherDashboard_Shown(object? sender, EventArgs e)
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
                AppLogger.Error("TeacherDashboard", "MQTT subscribe failed.", ex);
                MessageBox.Show(
                    $"MQTT subscribe failed: {ex.Message}",
                    "MQTT",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
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
                UpdateTeacherKpis();
                if (_studentVitalsTrendChart != null)
                {
                    DrawLineSeries(_studentVitalsTrendChart, Array.Empty<TrendSeriesData>());
                }
                if (_studentConfidenceTrendChart != null)
                {
                    DrawLineSeries(_studentConfidenceTrendChart, Array.Empty<TrendSeriesData>());
                }
                LoadAlertHistory();
                return;
            }

            using var db = new AppDbContext();
            var utcNow = DateTime.UtcNow;
            var filteredQuery = ApplyStudentSensorFilters(
                db.SensorReadings.Where(r => r.DeviceId == _selectedStudentDeviceId),
                utcNow);

            var rows = filteredQuery
                .OrderByDescending(r => r.ReceivedAtUtc)
                .Take(SensorPageSize)
                .Select(r => new
                {
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
            dgvSensorReadings.Visible = true;
            ApplySensorGestureChipStyles();

            var latestReading = filteredQuery
                .OrderByDescending(r => r.ReceivedAtUtc)
                .FirstOrDefault();
            UpdateTeacherKpis(latestReading, filteredQuery);
            UpdateTeacherTrendCharts(filteredQuery, utcNow);
            LoadAlertHistory();
        }

        private IQueryable<SensorReading> ApplyStudentSensorFilters(IQueryable<SensorReading> query, DateTime utcNow)
        {
            if (_studentSensorWindow is TimeSpan window)
            {
                var startUtc = utcNow - window;
                query = query.Where(r => r.ReceivedAtUtc >= startUtc);
            }

            if (!string.IsNullOrWhiteSpace(_studentSensorSearchTerm))
            {
                query = query.Where(r =>
                    (r.HandGesture != null && r.HandGesture.Contains(_studentSensorSearchTerm)) ||
                    r.DeviceId.Contains(_studentSensorSearchTerm));
            }

            switch (_studentGestureFilter)
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

            if (_studentSensorAlertOnly)
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
            CreateTeacherKpiCards();
            CreateStudentSensorFilterControls();
            CreateTrendCharts();
            UpdateTeacherKpis();
        }

        private void CreateThemeToggleControl()
        {
            _themeToggleCheckBox = new CheckBox
            {
                Location = new Point(912, 17),
                Size = new Size(123, 30),
                Text = "Dark theme",
                Checked = _isDarkTheme,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };

            _themeToggleCheckBox.CheckedChanged += (_, _) =>
            {
                if (_isThemeApplying)
                {
                    return;
                }

                _isDarkTheme = _themeToggleCheckBox.Checked;
                _currentTeacher.ThemePreference = _isDarkTheme ? ThemeDark : ThemeLight;
                SaveThemePreference();
                ApplyModernTheme();
            };

            Controls.Add(_themeToggleCheckBox);
            _themeToggleCheckBox.BringToFront();

            _uiToolTip.SetToolTip(_themeToggleCheckBox, "Switch dashboard between light and dark themes.");
        }

        private void CreateTeacherKpiCards()
        {
            var panel = new FlowLayoutPanel
            {
                Location = new Point(756, 80),
                Size = new Size(416, 86),
                BackColor = Color.Transparent,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };

            _kpiStudentReadingsLabel = CreateKpiChip("Connected students: -", 204);
            _kpiLatestGestureLabel = CreateKpiChip("Last gesture: -", 204);
            _kpiGestureQualityLabel = CreateKpiChip("Avg confidence: -", 204);
            _kpiStudentAlertsLabel = CreateKpiChip("Active alerts: -", 204);

            panel.Controls.Add(_kpiStudentReadingsLabel);
            panel.Controls.Add(_kpiLatestGestureLabel);
            panel.Controls.Add(_kpiGestureQualityLabel);
            panel.Controls.Add(_kpiStudentAlertsLabel);

            Controls.Add(panel);
            panel.BringToFront();
        }

        private void CreateStudentSensorFilterControls()
        {
            _studentSensorSearchTextBox = new TextBox
            {
                Location = new Point(775, 236),
                Size = new Size(120, 31),
                PlaceholderText = "Search",
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };
            _studentSensorSearchTextBox.TextChanged += (_, _) =>
            {
                _studentSensorSearchTerm = _studentSensorSearchTextBox.Text.Trim();
                LoadSensorReadingsForSelectedStudent();
            };

            _studentGestureComboBox = new ComboBox
            {
                Location = new Point(901, 236),
                Size = new Size(88, 31),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };
            _studentGestureComboBox.Items.AddRange(new object[]
            {
                "All Gestures",
                "Open Palm",
                "Fist",
                "No Hand",
            });
            _studentGestureComboBox.SelectedIndex = 0;
            _studentGestureComboBox.SelectedIndexChanged += (_, _) =>
            {
                _studentGestureFilter = _studentGestureComboBox.SelectedItem?.ToString() ?? "All Gestures";
                LoadSensorReadingsForSelectedStudent();
            };

            _studentSensorWindowComboBox = new ComboBox
            {
                Location = new Point(995, 236),
                Size = new Size(78, 31),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };
            _studentSensorWindowComboBox.Items.AddRange(new object[]
            {
                "30m",
                "6h",
                "24h",
                "All",
            });
            _studentSensorWindowComboBox.SelectedIndex = 1;
            _studentSensorWindowComboBox.SelectedIndexChanged += (_, _) =>
            {
                _studentSensorWindow = _studentSensorWindowComboBox.SelectedIndex switch
                {
                    0 => TimeSpan.FromMinutes(30),
                    1 => TimeSpan.FromHours(6),
                    2 => TimeSpan.FromHours(24),
                    _ => null,
                };
                LoadSensorReadingsForSelectedStudent();
            };

            _studentSensorAlertOnlyCheckBox = new CheckBox
            {
                Location = new Point(1079, 238),
                Size = new Size(50, 28),
                Text = "Alert",
                AutoSize = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };
            _studentSensorAlertOnlyCheckBox.CheckedChanged += (_, _) =>
            {
                _studentSensorAlertOnly = _studentSensorAlertOnlyCheckBox.Checked;
                LoadSensorReadingsForSelectedStudent();
            };

            _studentFilterClearButton = new Button
            {
                Location = new Point(1116, 234),
                Size = new Size(56, 35),
                Text = "Clear",
                UseVisualStyleBackColor = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };
            _studentFilterClearButton.Click += (_, _) =>
            {
                _studentSensorSearchTextBox.Text = string.Empty;
                _studentGestureComboBox.SelectedIndex = 0;
                _studentSensorWindowComboBox.SelectedIndex = 1;
                _studentSensorAlertOnlyCheckBox.Checked = false;
            };

            Controls.Add(_studentSensorSearchTextBox);
            Controls.Add(_studentGestureComboBox);
            Controls.Add(_studentSensorWindowComboBox);
            Controls.Add(_studentSensorAlertOnlyCheckBox);
            Controls.Add(_studentFilterClearButton);

            _studentSensorSearchTextBox.BringToFront();
            _studentGestureComboBox.BringToFront();
            _studentSensorWindowComboBox.BringToFront();
            _studentSensorAlertOnlyCheckBox.BringToFront();
            _studentFilterClearButton.BringToFront();

            _uiToolTip.SetToolTip(_studentSensorSearchTextBox, "Search readings by gesture or device.");
            _uiToolTip.SetToolTip(_studentGestureComboBox, "Filter by gesture type.");
            _uiToolTip.SetToolTip(_studentSensorWindowComboBox, "Filter by time range.");
            _uiToolTip.SetToolTip(_studentSensorAlertOnlyCheckBox, "Show only alert-condition readings.");
        }

        private void CreateTrendCharts()
        {
            _studentVitalsTrendChart = CreateMiniTrendChart(new Point(775, 170), new Size(397, 28));
            _studentConfidenceTrendChart = CreateMiniTrendChart(new Point(775, 202), new Size(397, 28));

            Controls.Add(_studentVitalsTrendChart);
            Controls.Add(_studentConfidenceTrendChart);

            _studentVitalsTrendChart.BringToFront();
            _studentConfidenceTrendChart.BringToFront();

            _uiToolTip.SetToolTip(_studentVitalsTrendChart, "Vitals trend (HR / SpO2 / Temp), last 30 minutes.");
            _uiToolTip.SetToolTip(_studentConfidenceTrendChart, "Gesture confidence trend, last 30 minutes.");
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

        private void UpdateTeacherTrendCharts(IQueryable<SensorReading> filteredQuery, DateTime utcNow)
        {
            if (_studentVitalsTrendChart == null || _studentConfidenceTrendChart == null)
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
                _studentVitalsTrendChart,
                new[]
                {
                    BuildSeriesData("HR", ColorTranslator.FromHtml("#2563EB"), sample.Select(r => (r.ReceivedAtUtc, r.HeartRate))),
                    BuildSeriesData("SpO2", ColorTranslator.FromHtml("#16A34A"), sample.Select(r => (r.ReceivedAtUtc, r.Spo2))),
                    BuildSeriesData("Temp", ColorTranslator.FromHtml("#DC2626"), sample.Select(r => (r.ReceivedAtUtc, r.BodyTemperatureC))),
                });

            DrawLineSeries(
                _studentConfidenceTrendChart,
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
                var teacher = db.Users.FirstOrDefault(u => u.Id == _currentTeacher.Id);
                if (teacher == null)
                {
                    return;
                }

                teacher.ThemePreference = _isDarkTheme ? ThemeDark : ThemeLight;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                AppLogger.Warning("TeacherDashboard", $"Failed to save theme preference: {ex.Message}");
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
            dgvSensorReadings.Size = new Size(760, 346);

            var titleLabel = new Label
            {
                Location = new Point(780, 525),
                Size = new Size(118, 30),
                Text = "Alert History",
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point),
            };

            _alertHistoryStatusComboBox = new ComboBox
            {
                Location = new Point(900, 523),
                Size = new Size(132, 31),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };
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

            _alertHistoryRefreshButton = new Button
            {
                Location = new Point(1038, 521),
                Size = new Size(77, 35),
                Text = "Refresh",
                UseVisualStyleBackColor = false,
            };
            _alertHistoryRefreshButton.Click += (_, _) => LoadAlertHistory();

            _alertHistoryCountLabel = new Label
            {
                Location = new Point(1121, 525),
                Size = new Size(51, 30),
                Text = "0",
                TextAlign = ContentAlignment.MiddleRight,
            };

            _alertHistoryGrid = new DataGridView
            {
                Location = new Point(780, 561),
                Size = new Size(392, 346),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                BorderStyle = BorderStyle.FixedSingle,
            };
            _alertHistoryGrid.SizeChanged += (_, _) => ApplyAlertHistoryColumnVisibility();

            Controls.Add(titleLabel);
            Controls.Add(_alertHistoryStatusComboBox);
            Controls.Add(_alertHistoryRefreshButton);
            Controls.Add(_alertHistoryCountLabel);
            Controls.Add(_alertHistoryGrid);

            titleLabel.BringToFront();
            _alertHistoryStatusComboBox.BringToFront();
            _alertHistoryRefreshButton.BringToFront();
            _alertHistoryCountLabel.BringToFront();

            _uiToolTip.SetToolTip(_alertHistoryStatusComboBox, "Filter alert history by status.");
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
                var query = db.AlertOutboxMessages
                    .Where(item => item.Source == nameof(TeacherDashboard));

                if (!string.IsNullOrWhiteSpace(_selectedStudentDeviceId))
                {
                    query = query.Where(item => item.DeviceId == _selectedStudentDeviceId);
                }

                var statusFilter = _alertHistoryStatusComboBox.SelectedItem?.ToString();
                if (!string.IsNullOrWhiteSpace(statusFilter)
                    && !statusFilter.StartsWith("All", StringComparison.OrdinalIgnoreCase)
                    && Enum.TryParse<AlertOutboxStatus>(statusFilter, out var parsedStatus))
                {
                    query = query.Where(item => item.Status == parsedStatus);
                }

                var rows = query
                    .OrderByDescending(item => item.CreatedAtUtc)
                    .Take(120)
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
                _alertHistoryCountLabel.Text = rows.Count.ToString();
            }
            catch (Exception ex)
            {
                AppLogger.Warning("TeacherDashboard", $"Alert history load failed: {ex.Message}");
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

            created.Width = 78;
            device.Width = 72;
            severity.Width = 58;
            status.Width = 70;
            attempts.Width = 42;
            nextRetry.Width = 94;
            sent.Width = 74;
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

            attempts.Visible = width >= 300;
            sent.Visible = width >= 360;
            nextRetry.Visible = width >= 430;
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

        private void UpdateTeacherKpis(SensorReading? latestReading = null, IQueryable<SensorReading>? filteredQuery = null)
        {
            if (_kpiStudentReadingsLabel == null ||
                _kpiLatestGestureLabel == null ||
                _kpiGestureQualityLabel == null ||
                _kpiStudentAlertsLabel == null)
            {
                return;
            }

            using var db = new AppDbContext();

            var connectedCutoffUtc = DateTime.UtcNow - StudentOnlineWindow;
            var connectedStudents = db.SensorReadings
                .Where(r => r.ReceivedAtUtc >= connectedCutoffUtc)
                .Select(r => r.DeviceId)
                .Distinct()
                .Count();

            var latestByDevice = db.SensorReadings
                .OrderByDescending(r => r.ReceivedAtUtc)
                .Take(2000)
                .AsEnumerable()
                .GroupBy(r => r.DeviceId, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();

            var activeAlerts = latestByDevice.Count(IsAlertCondition);

            if (string.IsNullOrWhiteSpace(_selectedStudentDeviceId))
            {
                _kpiStudentReadingsLabel.Text = $"Connected students: {connectedStudents}";
                _kpiLatestGestureLabel.Text = "Last gesture: -";
                _kpiGestureQualityLabel.Text = "Avg confidence: -";
                _kpiStudentAlertsLabel.Text = $"Active alerts: {activeAlerts}";
                return;
            }

            var query = filteredQuery ?? ApplyStudentSensorFilters(
                db.SensorReadings.Where(r => r.DeviceId == _selectedStudentDeviceId),
                DateTime.UtcNow);

            var sample = query
                .OrderByDescending(r => r.ReceivedAtUtc)
                .Take(200)
                .ToList();

            latestReading ??= sample.FirstOrDefault();
            var avgConfidence = sample
                .Where(r => r.HandGestureConfidence.HasValue)
                .Select(r => r.HandGestureConfidence!.Value)
                .DefaultIfEmpty()
                .Average();

            _kpiStudentReadingsLabel.Text = $"Connected students: {connectedStudents}";
            _kpiLatestGestureLabel.Text = latestReading == null
                ? "Last gesture: -"
                : $"Last gesture: {latestReading.HandGesture ?? "n/a"}";
            _kpiGestureQualityLabel.Text = avgConfidence <= 0
                ? "Avg confidence: -"
                : $"Avg confidence: {avgConfidence:0.00}";
            _kpiStudentAlertsLabel.Text = $"Active alerts: {activeAlerts}";
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
                Height = 34,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 0, 8, 8),
                Padding = new Padding(8, 0, 8, 0),
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point),
                BorderStyle = BorderStyle.FixedSingle,
            };
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
            lblDecisionStatus.Text = "Decision: select a student to view gesture telemetry";
            lblDecisionStatus.ForeColor = Color.Black;
            lblConnectionStatus.Text = "Connection: select a student";
            lblConnectionStatus.ForeColor = Color.DimGray;

            dgvSensorReadings.DataSource = null;
            dgvSensorReadings.Visible = false;
            UpdateTeacherKpis();
            LoadAlertHistory();
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
            return $"camera-{clean}-{token}";
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

            lblDecisionStatus.Text = "Decision: waiting for gesture telemetry...";
            lblDecisionStatus.ForeColor = Color.Black;
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
                AppLogger.Warning("TeacherDashboard", "Invalid telemetry payload.", ex, eventName: "mqtt_payload_invalid", context: new { topic, PayloadLength = payload.Length });
                BeginInvoke(() => lblDecisionStatus.Text = $"Decision: invalid payload ({ex.Message})");
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
                if (_alertOutboxService.TryEnqueueFromDecision(reading, decision, nameof(TeacherDashboard), out var queuedMessage)
                    && queuedMessage != null)
                {
                    AppLogger.Info("TeacherDashboard", "Alert queued for delivery.", eventName: "alert_outbox_queued", context: new { queuedMessage.Id, queuedMessage.Severity, queuedMessage.DeviceId });
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("TeacherDashboard", "Failed to queue alert into outbox.", ex);
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

























































