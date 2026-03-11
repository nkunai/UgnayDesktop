using UgnayDesktop.Services;

namespace UgnayDesktop.Forms;

public partial class TeacherDashboard
{
    private GestureRuntimeService? _gestureRuntimeService;
    private GloveSpeechService? _gloveSpeechService;
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
    private CheckBox? _enableSpeechCheckBox;
    private Button? _startCameraButton;
    private Button? _stopCameraButton;
    private DateTime _lastPreviewWriteUtc = DateTime.MinValue;
    private string? _lastObservedLabel;
    private string? _lastSpokenLabel;
    private int _samePredictionCount;
    private DateTime _lastSpokenAtUtc = DateTime.MinValue;

    private void InitializeGestureStage3Ui()
    {
        ClientSize = new Size(Math.Max(ClientSize.Width, 1600), ClientSize.Height);
        MinimumSize = new Size(1500, 960);

        btnLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _gloveSpeechService = new GloveSpeechService();

        _gestureGroup = new GroupBox
        {
            Text = "Stage 3 Gesture Test",
            Location = new Point(1190, 80),
            Size = new Size(390, 827),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right
        };

        _gesturePreview = new PictureBox
        {
            Location = new Point(18, 36),
            Size = new Size(350, 260),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Black,
            BorderStyle = BorderStyle.FixedSingle
        };

        var cameraLabel = new Label
        {
            Text = "Camera Index",
            Location = new Point(18, 316),
            AutoSize = true
        };

        _cameraIndexInput = new NumericUpDown
        {
            Location = new Point(18, 344),
            Size = new Size(90, 35),
            Minimum = 0,
            Maximum = 5,
            Value = 0
        };

        _startCameraButton = new Button
        {
            Text = "Start Camera",
            Location = new Point(126, 340),
            Size = new Size(115, 40)
        };
        _startCameraButton.Click += (_, _) => StartGestureStage3();

        _stopCameraButton = new Button
        {
            Text = "Stop Camera",
            Location = new Point(253, 340),
            Size = new Size(115, 40),
            Enabled = false
        };
        _stopCameraButton.Click += (_, _) => StopGestureStage3();

        _gestureStatusLabel = BuildStage3Label("Status: idle", 396);
        _gestureModelLabel = BuildStage3Label("Model: waiting", 436);
        _gesturePredictionLabel = BuildStage3Label("Prediction: waiting", 476, bold: true);
        _gestureMovementLabel = BuildStage3Label("Movement: 0.0000", 516);
        _gestureConfidenceLabel = BuildStage3Label("Confidence: 0.00", 556);

        var gloveIpLabel = new Label
        {
            Text = "Left Glove IP",
            Location = new Point(18, 604),
            AutoSize = true
        };

        _gloveIpTextBox = new TextBox
        {
            Location = new Point(18, 632),
            Size = new Size(180, 35),
            Text = "192.168.1.101"
        };

        _enableSpeechCheckBox = new CheckBox
        {
            Location = new Point(216, 634),
            Size = new Size(152, 30),
            Text = "Speak to glove",
            Checked = true
        };

        _speechStatusLabel = new Label
        {
            Location = new Point(18, 678),
            Size = new Size(350, 60),
            Text = "Speech: waiting for a stable gesture"
        };

        var helperLabel = new Label
        {
            Location = new Point(18, 748),
            Size = new Size(350, 58),
            Text = "The app speaks only after a stable recognition. Audio is synthesized on the laptop and streamed back to the left glove speaker over UDP port 5006."
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
        _gestureGroup.Controls.Add(gloveIpLabel);
        _gestureGroup.Controls.Add(_gloveIpTextBox);
        _gestureGroup.Controls.Add(_enableSpeechCheckBox);
        _gestureGroup.Controls.Add(_speechStatusLabel);
        _gestureGroup.Controls.Add(helperLabel);
        Controls.Add(_gestureGroup);

        FormClosed += (_, _) => DisposeGestureStage3();
        Shown += (_, _) => StartGestureStage3();

        _gestureUiTimer = new System.Windows.Forms.Timer { Interval = 180 };
        _gestureUiTimer.Tick += (_, _) => RefreshGestureStage3Ui();
        _gestureUiTimer.Start();
    }

    private Label BuildStage3Label(string text, int top, bool bold = false)
    {
        return new Label
        {
            Location = new Point(18, top),
            Size = new Size(350, 30),
            Font = bold ? new Font(Font, FontStyle.Bold) : Font,
            Text = text
        };
    }

    private void StartGestureStage3()
    {
        try
        {
            _gestureRuntimeService ??= new GestureRuntimeService();
            _gestureRuntimeService.Start((int)(_cameraIndexInput?.Value ?? 0));
            if (_startCameraButton != null) _startCameraButton.Enabled = false;
            if (_stopCameraButton != null) _stopCameraButton.Enabled = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Camera start failed: {ex.Message}", "Stage 3", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            if (_gestureStatusLabel != null)
            {
                _gestureStatusLabel.Text = $"Status: failed ({ex.Message})";
            }
        }
    }

    private void StopGestureStage3()
    {
        _gestureRuntimeService?.Stop();
        if (_startCameraButton != null) _startCameraButton.Enabled = true;
        if (_stopCameraButton != null) _stopCameraButton.Enabled = false;
        if (_speechStatusLabel != null) _speechStatusLabel.Text = "Speech: stopped";
    }

    private void RefreshGestureStage3Ui()
    {
        if (_gestureRuntimeService == null)
        {
            return;
        }

        var snapshot = _gestureRuntimeService.LatestSnapshot;
        if (_gestureStatusLabel != null) _gestureStatusLabel.Text = $"Status: {snapshot.Status}";
        if (_gestureModelLabel != null) _gestureModelLabel.Text = $"Model: {snapshot.ActiveModel}";
        if (_gesturePredictionLabel != null) _gesturePredictionLabel.Text = $"Prediction: {snapshot.PredictedLabel}";
        if (_gestureMovementLabel != null) _gestureMovementLabel.Text = $"Movement: {snapshot.MovementScore:0.0000}";
        if (_gestureConfidenceLabel != null) _gestureConfidenceLabel.Text = $"Confidence: {snapshot.Confidence:0.00} | Distance: {snapshot.Distance:0.00}";

        RefreshGesturePreview(snapshot.PreviewImagePath);
        TrySpeakRecognizedGesture(snapshot);
    }

    private async void TrySpeakRecognizedGesture(GestureRuntimeSnapshot snapshot)
    {
        if (_gloveSpeechService == null || _enableSpeechCheckBox?.Checked != true || _gloveIpTextBox == null)
        {
            return;
        }

        var label = snapshot.PredictedLabel?.Trim();
        if (string.IsNullOrWhiteSpace(label) ||
            label.StartsWith("No hand", StringComparison.OrdinalIgnoreCase) ||
            label.StartsWith("Waiting", StringComparison.OrdinalIgnoreCase) ||
            snapshot.Confidence < 0.22)
        {
            _samePredictionCount = 0;
            _lastObservedLabel = null;
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

        if (string.Equals(label, _lastSpokenLabel, StringComparison.OrdinalIgnoreCase) &&
            DateTime.UtcNow - _lastSpokenAtUtc < TimeSpan.FromSeconds(3))
        {
            return;
        }

        try
        {
            if (_speechStatusLabel != null)
            {
                _speechStatusLabel.Text = $"Speech: sending {label} to {_gloveIpTextBox.Text.Trim()}";
            }

            await _gloveSpeechService.SpeakAsync(label, _gloveIpTextBox.Text.Trim());
            _lastSpokenLabel = label;
            _lastSpokenAtUtc = DateTime.UtcNow;

            if (_speechStatusLabel != null)
            {
                _speechStatusLabel.Text = $"Speech: sent {label}";
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

    private void RefreshGesturePreview(string previewPath)
    {
        if (_gesturePreview == null || string.IsNullOrWhiteSpace(previewPath) || !File.Exists(previewPath))
        {
            return;
        }

        var lastWrite = File.GetLastWriteTimeUtc(previewPath);
        if (lastWrite <= _lastPreviewWriteUtc)
        {
            return;
        }

        using var stream = new FileStream(previewPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var image = Image.FromStream(stream);
        var nextFrame = new Bitmap(image);
        var previous = _gesturePreview.Image;
        _gesturePreview.Image = nextFrame;
        previous?.Dispose();
        _lastPreviewWriteUtc = lastWrite;
    }

    private void DisposeGestureStage3()
    {
        _gestureUiTimer?.Stop();
        _gestureUiTimer?.Dispose();
        _gestureUiTimer = null;

        _gestureRuntimeService?.Dispose();
        _gestureRuntimeService = null;

        _gloveSpeechService?.Dispose();
        _gloveSpeechService = null;

        if (_gesturePreview?.Image != null)
        {
            _gesturePreview.Image.Dispose();
            _gesturePreview.Image = null;
        }
    }
}
