using System.Text.Json;
using UgnayDesktop.Services;

namespace UgnayDesktop.Forms;

public partial class TeacherDashboard
{
    private const int Stage3CalibrationSamples = 60;
    private const int Stage3StackLayoutThreshold = 1500;
    private const int Stage3PanelWidth = 460;
    private static readonly TimeSpan Stage3PhraseCommitDelay = TimeSpan.FromMilliseconds(1200);
    private static readonly TimeSpan Stage3RepeatedLabelGap = TimeSpan.FromMilliseconds(900);
    private static readonly IReadOnlyDictionary<string, string> Stage3PhraseMap = LoadStage3PhraseMap();

    private GestureRuntimeService? _gestureRuntimeService;
    private GloveSpeechService? _gloveSpeechService;
    private GloveStartupService? _gloveStartupService;
    private System.Windows.Forms.Timer? _gestureUiTimer;
    private GroupBox? _gestureGroup;
    private PictureBox? _gesturePreview;
    private Label? _gestureStatusLabel;
    private Label? _gesturePredictionLabel;
    private Label? _gestureModelLabel;
    private Label? _gestureMovementLabel;
    private Label? _gestureConfidenceLabel;
    private Label? _speechStatusLabel;
    private NumericUpDown? _cameraIndexInput;
    private TextBox? _gloveIpTextBox;
    private TextBox? _rightGloveIpTextBox;
    private CheckBox? _enableSpeechCheckBox;
    private Button? _startCameraButton;
    private Button? _stopCameraButton;
    private CancellationTokenSource? _stage3StartupCts;
    private bool _stage3Starting;
    private string? _lastObservedLabel;
    private readonly List<string> _pendingSpeechTokens = new();
    private string? _lastCommittedSpeechLabel;
    private string? _lastSpokenPhrase;
    private int _samePredictionCount;
    private DateTime _lastCommittedSpeechAtUtc = DateTime.MinValue;
    private DateTime _lastSpokenAtUtc = DateTime.MinValue;

    private void InitializeGestureStage3Ui()
    {
        AutoScroll = true;
        MinimumSize = new Size(1120, 860);
        ApplyTeacherDashboardBounds();

        btnLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _gloveSpeechService = new GloveSpeechService();

        _gestureGroup = new GroupBox
        {
            Text = "Stage 3 Gesture Test"
        };

        _gesturePreview = new PictureBox
        {
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Black,
            BorderStyle = BorderStyle.FixedSingle
        };

        var cameraLabel = new Label { Text = "Camera Index", AutoSize = true };
        _cameraIndexInput = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 5,
            Value = 0
        };

        _startCameraButton = new Button { Text = "Start" };
        _startCameraButton.Click += (_, _) => StartGestureStage3();

        _stopCameraButton = new Button
        {
            Text = "Stop",
            Enabled = false
        };
        _stopCameraButton.Click += (_, _) => StopGestureStage3();

        _gestureStatusLabel = BuildStage3Label("Status: idle");
        _gestureModelLabel = BuildStage3Label("Gloves: waiting");
        _gesturePredictionLabel = BuildStage3Label("Prediction: waiting", bold: true);
        _gestureMovementLabel = BuildStage3Label("Movement: 0.0000");
        _gestureConfidenceLabel = BuildStage3Label("Confidence: 0.00");

        var leftGloveIpLabel = new Label { Text = "Left Glove IP", AutoSize = true };
        _gloveIpTextBox = new TextBox { Text = "192.168.100.151" };

        var rightGloveIpLabel = new Label { Text = "Right Glove IP", AutoSize = true };
        _rightGloveIpTextBox = new TextBox { Text = "192.168.100.150" };

        _enableSpeechCheckBox = new CheckBox
        {
            Text = "Speak to glove",
            Checked = true
        };

        _speechStatusLabel = new Label
        {
            Text = "Speech: waiting for a stable gesture"
        };

        var helperLabel = new Label
        {
            Name = "lblStage3Helper",
            Text = "The app now waits for both gloves, calibrates first, then starts the camera. Audio is synthesized on the laptop and streamed back to the left glove speaker over UDP port 5006."
        };

        _gestureGroup.Controls.Add(_gesturePreview);
        _gestureGroup.Controls.Add(cameraLabel);
        _gestureGroup.Controls.Add(_cameraIndexInput);
        _gestureGroup.Controls.Add(_startCameraButton);
        _gestureGroup.Controls.Add(_stopCameraButton);
        _gestureGroup.Controls.Add(_gestureStatusLabel);
        _gestureGroup.Controls.Add(_gestureModelLabel);
        _gestureGroup.Controls.Add(_gesturePredictionLabel);
        _gestureGroup.Controls.Add(_gestureMovementLabel);
        _gestureGroup.Controls.Add(_gestureConfidenceLabel);
        _gestureGroup.Controls.Add(leftGloveIpLabel);
        _gestureGroup.Controls.Add(_gloveIpTextBox);
        _gestureGroup.Controls.Add(rightGloveIpLabel);
        _gestureGroup.Controls.Add(_rightGloveIpTextBox);
        _gestureGroup.Controls.Add(_enableSpeechCheckBox);
        _gestureGroup.Controls.Add(_speechStatusLabel);
        _gestureGroup.Controls.Add(helperLabel);
        Controls.Add(_gestureGroup);

        Resize += (_, _) => LayoutGestureStage3Ui();
        FormClosed += (_, _) => DisposeGestureStage3();
        Shown += (_, _) => StartGestureStage3();

        _gestureUiTimer = new System.Windows.Forms.Timer { Interval = 100 };
        _gestureUiTimer.Tick += (_, _) => RefreshGestureStage3Ui();
        _gestureUiTimer.Start();

        LayoutGestureStage3Ui();
    }

    private Label BuildStage3Label(string text, bool bold = false)
    {
        return new Label
        {
            AutoSize = false,
            Font = bold ? new Font(Font, FontStyle.Bold) : Font,
            Text = text
        };
    }

    private void ApplyTeacherDashboardBounds()
    {
        var workingArea = Screen.FromPoint(Location).WorkingArea;
        var targetWidth = Math.Min(workingArea.Width - 40, 1480);
        var targetHeight = Math.Min(workingArea.Height - 40, 960);
        Size = new Size(Math.Max(1184, targetWidth), Math.Max(900, targetHeight));
    }

    private void LayoutGestureStage3Ui()
    {
        if (_gestureGroup == null || _gesturePreview == null || _cameraIndexInput == null ||
            _startCameraButton == null || _stopCameraButton == null || _gloveIpTextBox == null ||
            _rightGloveIpTextBox == null || _enableSpeechCheckBox == null || _speechStatusLabel == null)
        {
            return;
        }

        const int margin = 12;
        const int gap = 18;
        var stacked = ClientSize.Width < Stage3StackLayoutThreshold;
        var mainWidth = stacked
            ? Math.Max(980, ClientSize.Width - (margin * 2) - 16)
            : Math.Max(760, ClientSize.Width - (margin * 2) - gap - Stage3PanelWidth);

        dgvStudents.Width = mainWidth;
        dgvSensorReadings.Width = mainWidth;
        lblSelectedStudent.MaximumSize = new Size(mainWidth, 0);
        lblConnectionStatus.MaximumSize = new Size(mainWidth, 0);
        lblDecisionStatus.MaximumSize = new Size(mainWidth, 0);

        var rightEdge = margin + mainWidth;
        btnLogout.Left = rightEdge - btnLogout.Width;
        btnTwilioTest.Left = btnLogout.Left - 8 - btnTwilioTest.Width;
        btnTwilioLink.Left = btnTwilioTest.Left - 8 - btnTwilioLink.Width;
        btnTwilioConfigCheck.Left = btnTwilioLink.Left - 8 - btnTwilioConfigCheck.Width;

        if (stacked)
        {
            _gestureGroup.Location = new Point(margin, dgvSensorReadings.Bottom + gap);
            _gestureGroup.Size = new Size(mainWidth, 700);
        }
        else
        {
            _gestureGroup.Location = new Point(margin + mainWidth + gap, 80);
            _gestureGroup.Size = new Size(Stage3PanelWidth, ClientSize.Height - 92);
        }

        var innerLeft = 18;
        var innerTop = 36;
        var innerWidth = _gestureGroup.ClientSize.Width - (innerLeft * 2);
        var previewSize = stacked
            ? Math.Min(innerWidth, 360)
            : Math.Min(innerWidth, 424);

        _gesturePreview.Location = new Point(innerLeft, innerTop);
        _gesturePreview.Size = new Size(previewSize, previewSize);

        var controlsTop = _gesturePreview.Bottom + 16;
        var cameraLabel = _gestureGroup.Controls.OfType<Label>().First(label => label.Text == "Camera Index");
        cameraLabel.Location = new Point(innerLeft, controlsTop);

        _cameraIndexInput.Location = new Point(innerLeft, cameraLabel.Bottom + 6);
        _cameraIndexInput.Size = new Size(90, 35);

        _startCameraButton.Location = new Point(_cameraIndexInput.Right + 12, _cameraIndexInput.Top - 2);
        _startCameraButton.Size = new Size(100, 40);
        _stopCameraButton.Location = new Point(_startCameraButton.Right + 10, _cameraIndexInput.Top - 2);
        _stopCameraButton.Size = new Size(100, 40);
        var statusTop = _startCameraButton.Bottom + 14;
        LayoutStage3Label(_gestureStatusLabel!, innerLeft, statusTop, innerWidth, 48);
        LayoutStage3Label(_gestureModelLabel!, innerLeft, _gestureStatusLabel!.Bottom + 6, innerWidth, 48);
        LayoutStage3Label(_gesturePredictionLabel!, innerLeft, _gestureModelLabel!.Bottom + 6, innerWidth, 48);
        LayoutStage3Label(_gestureMovementLabel!, innerLeft, _gesturePredictionLabel!.Bottom + 6, innerWidth, 34);
        LayoutStage3Label(_gestureConfidenceLabel!, innerLeft, _gestureMovementLabel!.Bottom + 6, innerWidth, 40);

        var leftGloveIpLabel = _gestureGroup.Controls.OfType<Label>().First(label => label.Text == "Left Glove IP");
        leftGloveIpLabel.Location = new Point(innerLeft, _gestureConfidenceLabel!.Bottom + 16);
        _gloveIpTextBox.Location = new Point(innerLeft, leftGloveIpLabel.Bottom + 6);
        _gloveIpTextBox.Size = new Size((innerWidth - 12) / 2, 35);

        var rightGloveIpLabel = _gestureGroup.Controls.OfType<Label>().First(label => label.Text == "Right Glove IP");
        rightGloveIpLabel.Location = new Point(_gloveIpTextBox.Right + 12, leftGloveIpLabel.Top);
        _rightGloveIpTextBox.Location = new Point(_gloveIpTextBox.Right + 12, _gloveIpTextBox.Top);
        _rightGloveIpTextBox.Size = new Size((innerWidth - 12) / 2, 35);

        _enableSpeechCheckBox.Location = new Point(innerLeft, _gloveIpTextBox.Bottom + 12);
        _enableSpeechCheckBox.Size = new Size(170, 30);

        _speechStatusLabel.Location = new Point(innerLeft, _enableSpeechCheckBox.Bottom + 10);
        _speechStatusLabel.Size = new Size(innerWidth, 42);

        var helperLabel = _gestureGroup.Controls.Find("lblStage3Helper", false).OfType<Label>().First();
        helperLabel.Location = new Point(innerLeft, _speechStatusLabel.Bottom + 8);
        helperLabel.Size = new Size(innerWidth, stacked ? 72 : 96);
    }

    private static void LayoutStage3Label(Label? label, int left, int top, int width, int height)
    {
        if (label == null)
        {
            return;
        }

        label.Location = new Point(left, top);
        label.Size = new Size(width, height);
    }

    private void StartGestureStage3()
    {
        if (_stage3Starting)
        {
            return;
        }

        StopGestureStage3(updateSpeechLabel: false);

        _stage3Starting = true;
        _stage3StartupCts = new CancellationTokenSource();
        var startupService = new GloveStartupService(_gloveIpTextBox?.Text, _rightGloveIpTextBox?.Text);
        _gloveStartupService = startupService;
        startupService.Start();

        if (_startCameraButton != null) _startCameraButton.Enabled = false;
        if (_stopCameraButton != null) _stopCameraButton.Enabled = true;
        if (_gestureStatusLabel != null) _gestureStatusLabel.Text = "Status: waiting for gloves";
        if (_speechStatusLabel != null) _speechStatusLabel.Text = "Speech: waiting for both gloves to come online";

        _ = RunGestureStartupAsync(startupService, (int)(_cameraIndexInput?.Value ?? 0), _stage3StartupCts.Token);
    }

    private async Task RunGestureStartupAsync(GloveStartupService startupService, int cameraIndex, CancellationToken token)
    {
        try
        {
            await startupService.WaitForBothGlovesAsync(token);

            if (_gestureStatusLabel != null)
            {
                BeginInvoke(() => _gestureStatusLabel.Text = "Status: calibrating gloves");
            }

            await startupService.SendCalibrationCommandAsync(token);
            await startupService.CalibrateAsync(Stage3CalibrationSamples, token);

            token.ThrowIfCancellationRequested();
            _gestureRuntimeService = new GestureRuntimeService();
            _gestureRuntimeService.Start(cameraIndex, Stage3DisplayMode.ExternalWindow);

            if (IsHandleCreated)
            {
                BeginInvoke(() =>
                {
                    if (_speechStatusLabel != null)
                    {
                        _speechStatusLabel.Text = "Speech: waiting for a stable gesture";
                    }
                });
            }
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
        catch (Exception ex)
        {
            if (IsHandleCreated)
            {
                BeginInvoke(() =>
                {
                    if (_gestureStatusLabel != null)
                    {
                        _gestureStatusLabel.Text = $"Status: failed ({ex.Message})";
                    }

                    if (_speechStatusLabel != null)
                    {
                        _speechStatusLabel.Text = "Speech: startup failed";
                    }
                });
            }
        }
        finally
        {
            _stage3Starting = false;

            if (_gestureRuntimeService == null)
            {
                if (IsHandleCreated)
                {
                    BeginInvoke(() =>
                    {
                        if (_startCameraButton != null) _startCameraButton.Enabled = true;
                        if (_stopCameraButton != null) _stopCameraButton.Enabled = false;
                    });
                }
            }
        }
    }

    private void StopGestureStage3(bool updateSpeechLabel = true)
    {
        _stage3StartupCts?.Cancel();
        _stage3StartupCts?.Dispose();
        _stage3StartupCts = null;
        _stage3Starting = false;

        _gestureRuntimeService?.Stop();
        _gestureRuntimeService?.Dispose();
        _gestureRuntimeService = null;

        _gloveStartupService?.Dispose();
        _gloveStartupService = null;

        if (_startCameraButton != null) _startCameraButton.Enabled = true;
        if (_stopCameraButton != null) _stopCameraButton.Enabled = false;
        _pendingSpeechTokens.Clear();
        _lastCommittedSpeechLabel = null;
        _lastObservedLabel = null;
        _samePredictionCount = 0;

        if (updateSpeechLabel && _speechStatusLabel != null) _speechStatusLabel.Text = "Speech: stopped";
        if (_gestureStatusLabel != null) _gestureStatusLabel.Text = "Status: stopped";
        if (_gesturePredictionLabel != null) _gesturePredictionLabel.Text = "Prediction: waiting";
    }

    private void RefreshGestureStage3Ui()
    {
        if (_gloveStartupService != null)
        {
            UpdateGloveStatus(_gloveStartupService.GetSnapshot());
        }

        if (_gestureRuntimeService == null)
        {
            return;
        }

        var snapshot = _gestureRuntimeService.LatestSnapshot;
        if (_gestureStatusLabel != null) _gestureStatusLabel.Text = $"Status: {snapshot.Status}";
        if (_gesturePredictionLabel != null) _gesturePredictionLabel.Text = $"Prediction: {snapshot.PredictedLabel}";
        if (_gestureMovementLabel != null) _gestureMovementLabel.Text = $"Movement: {snapshot.MovementScore:0.0000}";
        if (_gestureConfidenceLabel != null) _gestureConfidenceLabel.Text = $"Confidence: {snapshot.Confidence:0.00} | Distance: {snapshot.Distance:0.00}";
        TrySpeakRecognizedGesture(snapshot);
    }

    private void UpdateGloveStatus(GloveStartupSnapshot snapshot)
    {
        if (_gestureModelLabel != null)
        {
            _gestureModelLabel.Text = $"Gloves: Left {(snapshot.LeftOnline ? "online" : "offline")} | Right {(snapshot.RightOnline ? "online" : "offline")}";
        }

        if (_stage3Starting)
        {
            var target = snapshot.CalibrationTargetSamples;
            if (_gesturePredictionLabel != null)
            {
                _gesturePredictionLabel.Text = target > 0
                    ? $"Prediction: calibrating L {snapshot.CalibrationLeftSamples}/{target} | R {snapshot.CalibrationRightSamples}/{target}"
                    : $"Prediction: waiting for {(snapshot.LeftOnline ? string.Empty : "left glove ")}{(snapshot.RightOnline ? string.Empty : "right glove")}".Trim();
            }

            if (_gestureMovementLabel != null)
            {
                _gestureMovementLabel.Text = $"Left IP: {snapshot.LeftIp ?? "auto"}";
            }

            if (_gestureConfidenceLabel != null)
            {
                _gestureConfidenceLabel.Text = $"Right IP: {snapshot.RightIp ?? "auto"}";
            }
        }
        else if (_gestureRuntimeService != null && _gestureModelLabel != null)
        {
            _gestureModelLabel.Text += $" | Model: {_gestureRuntimeService.LatestSnapshot.ActiveModel}";
        }
    }

    private async void TrySpeakRecognizedGesture(GestureRuntimeSnapshot snapshot)
    {
        if (_gloveSpeechService == null || _enableSpeechCheckBox?.Checked != true || _gloveIpTextBox == null)
        {
            return;
        }

        var label = snapshot.PredictedLabel?.Trim();
        var nowUtc = DateTime.UtcNow;
        if (string.IsNullOrWhiteSpace(label) ||
            label.StartsWith("No hand", StringComparison.OrdinalIgnoreCase) ||
            label.StartsWith("Waiting", StringComparison.OrdinalIgnoreCase) ||
            snapshot.Confidence < 0.22)
        {
            _samePredictionCount = 0;
            _lastObservedLabel = null;
            await TrySpeakPendingPhraseAsync(nowUtc);
            return;
        }

        if (string.Equals(label, _lastObservedLabel, StringComparison.OrdinalIgnoreCase))
        {
            _samePredictionCount++;
        }
        else
        {
            _lastObservedLabel = label;
            _samePredictionCount = 1;
        }

        if (_samePredictionCount < 8)
        {
            if (_speechStatusLabel != null)
            {
                _speechStatusLabel.Text = $"Speech: stabilizing {label} ({_samePredictionCount}/8)";
            }
            return;
        }

        if (_samePredictionCount == 8)
        {
            var normalizedLabel = NormalizeSpeechToken(label);
            if (ShouldCommitSpeechToken(normalizedLabel, nowUtc))
            {
                _pendingSpeechTokens.Add(normalizedLabel);
                _lastCommittedSpeechLabel = normalizedLabel;
                _lastCommittedSpeechAtUtc = nowUtc;

                if (_speechStatusLabel != null)
                {
                    _speechStatusLabel.Text = $"Speech: building {string.Join(" + ", _pendingSpeechTokens)}";
                }
            }

            return;
        }

        await TrySpeakPendingPhraseAsync(nowUtc);
    }

    private bool ShouldCommitSpeechToken(string normalizedLabel, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(normalizedLabel))
        {
            return false;
        }

        if (!string.Equals(normalizedLabel, _lastCommittedSpeechLabel, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return nowUtc - _lastCommittedSpeechAtUtc >= Stage3RepeatedLabelGap;
    }

    private async Task TrySpeakPendingPhraseAsync(DateTime nowUtc)
    {
        if (_pendingSpeechTokens.Count == 0)
        {
            return;
        }

        if (nowUtc - _lastCommittedSpeechAtUtc < Stage3PhraseCommitDelay)
        {
            if (_speechStatusLabel != null)
            {
                _speechStatusLabel.Text = $"Speech: waiting for next sign ({string.Join(" + ", _pendingSpeechTokens)})";
            }
            return;
        }

        var phrase = BuildSpeechPhrase(_pendingSpeechTokens);
        _pendingSpeechTokens.Clear();
        _lastCommittedSpeechLabel = null;

        if (string.IsNullOrWhiteSpace(phrase))
        {
            return;
        }

        if (string.Equals(phrase, _lastSpokenPhrase, StringComparison.OrdinalIgnoreCase) &&
            nowUtc - _lastSpokenAtUtc < TimeSpan.FromSeconds(2))
        {
            return;
        }

        var targetIp = _gloveIpTextBox?.Text.Trim();
        if (string.IsNullOrWhiteSpace(targetIp))
        {
            return;
        }

        try
        {
            if (_speechStatusLabel != null)
            {
                _speechStatusLabel.Text = $"Speech: sending {phrase} to {targetIp}";
            }

            await _gloveSpeechService!.SpeakAsync(phrase, targetIp);
            _lastSpokenPhrase = phrase;
            _lastSpokenAtUtc = DateTime.UtcNow;

            if (_speechStatusLabel != null)
            {
                _speechStatusLabel.Text = $"Speech: sent {phrase}";
            }
        }
        catch (Exception ex)
        {
            if (_speechStatusLabel != null)
            {
                _speechStatusLabel.Text = $"Speech failed: {ex.Message}";
            }
        }
    }

    private static string BuildSpeechPhrase(IReadOnlyList<string> tokens)
    {
        if (tokens.Count == 0)
        {
            return string.Empty;
        }

        var normalizedTokens = tokens
            .Select(NormalizeSpeechToken)
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .ToList();
        if (normalizedTokens.Count == 0)
        {
            return string.Empty;
        }

        var letters = normalizedTokens.Where(IsAlphabetToken).ToList();
        string? spelledName = letters.Count > 0 ? string.Concat(letters) : null;
        var tokenKey = string.Join("|", normalizedTokens);

        if (Stage3PhraseMap.TryGetValue(tokenKey, out var mappedPhrase))
        {
            return mappedPhrase;
        }

        if (normalizedTokens.Count >= 2 &&
            string.Equals(normalizedTokens[0], "ako", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(normalizedTokens[1], "pangalan", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(spelledName))
        {
            return $"ako si {spelledName}";
        }

        if (!string.IsNullOrWhiteSpace(spelledName) && normalizedTokens.All(IsAlphabetToken))
        {
            return spelledName;
        }

        var nonAlphabetTokens = normalizedTokens.Where(token => !IsAlphabetToken(token)).ToList();
        if (nonAlphabetTokens.Count > 0 && !string.IsNullOrWhiteSpace(spelledName))
        {
            nonAlphabetTokens.Add(spelledName);
            return string.Join(' ', nonAlphabetTokens);
        }

        return string.Join(' ', normalizedTokens);
    }


    private static IReadOnlyDictionary<string, string> LoadStage3PhraseMap()
    {
        foreach (var candidate in GetStage3PhraseMapCandidates())
        {
            try
            {
                if (!File.Exists(candidate))
                {
                    continue;
                }

                var json = File.ReadAllText(candidate);
                var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (parsed == null || parsed.Count == 0)
                {
                    continue;
                }

                return new Dictionary<string, string>(parsed, StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                // Ignore malformed external config and fall back to defaults.
            }
        }

        return GetDefaultStage3PhraseMap();
    }

    private static IEnumerable<string> GetStage3PhraseMapCandidates()
    {
        yield return Path.Combine(AppContext.BaseDirectory, "stage3_phrase_rules.json");
        yield return Path.Combine(Directory.GetCurrentDirectory(), "stage3_phrase_rules.json");
        yield return Path.Combine(Directory.GetCurrentDirectory(), "GestureTrainer", "artifacts", "stage3_phrase_rules.json");
    }

    private static IReadOnlyDictionary<string, string> GetDefaultStage3PhraseMap()
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["pasensya"] = "pasensya",
            ["pasensya|ikaw"] = "pasensya ka na",
            ["kamusta"] = "kamusta",
            ["kamusta|ikaw"] = "kamusta ka",
            ["kamusta|ikaw|kamusta"] = "kamusta ka",
            ["magandang"] = "magandang",
            ["magandang|araw"] = "magandang araw",
            ["magandang|hapon"] = "magandang hapon",
            ["magandang|gabi"] = "magandang gabi",
            ["ulit"] = "ulit",
            ["ulit|ikaw"] = "ulitin mo nga",
            ["pangalan"] = "pangalan",
            ["walang anuman"] = "walang anuman",
            ["ano"] = "ano",
            ["ano|ikaw"] = "ano iyon",
            ["ano|pangalan|ikaw"] = "anong pangalan mo",
            ["pakiusap"] = "pakiusap",
            ["ako"] = "ako",
            ["ako|pangalan"] = "ako si",
            ["tara"] = "tara",
            ["tara|kain"] = "tara kain",
            ["tara|kain|ikaw"] = "tara kain ka",
            ["kain"] = "kain",
            ["kain|ikaw"] = "kain ka",
            ["sandali"] = "sandali lang",
            ["oo"] = "oo",
            ["hindi"] = "hindi",
            ["naiintindihan"] = "naiintindihan",
            ["naiintindihan|ikaw"] = "naiintindihan mo ba",
            ["siguro"] = "siguro",
            ["salamat"] = "salamat",
            ["salamat|ikaw"] = "salamat sa iyo",
            ["sino"] = "sino",
            ["sino|ikaw"] = "sino ka",
            ["ngayon"] = "ngayon",
            ["mamaya"] = "mamaya",
            ["mamaya|kain|ikaw"] = "mamaya kain ka",
            ["mamaya|ikaw|kain"] = "mamaya kain ka",
            ["kahapon"] = "kahapon",
            ["bukas"] = "bukas",
            ["kailan"] = "kailan",
            ["kailan|ikaw|kain"] = "kailan ka kakain",
            ["ganda"] = "ganda",
            ["ganda|ikaw"] = "ang ganda mo",
            ["mahal kita"] = "mahal kita",
        };
    }
    private static bool IsAlphabetToken(string token)
        => token.Length == 1 && token[0] >= 'a' && token[0] <= 'z';

    private static string NormalizeSpeechToken(string token)
    {
        var normalized = token.Trim().Replace('_', ' ').ToLowerInvariant();
        return normalized switch
        {
            "alphabet" => string.Empty,
            _ => normalized
        };
    }


    private void DisposeGestureStage3()
    {
        _gestureUiTimer?.Stop();
        _gestureUiTimer?.Dispose();
        _gestureUiTimer = null;

        StopGestureStage3(updateSpeechLabel: false);

        _gloveSpeechService?.Dispose();
        _gloveSpeechService = null;

        if (_gesturePreview?.Image != null)
        {
            _gesturePreview.Image.Dispose();
            _gesturePreview.Image = null;
        }
    }
}







