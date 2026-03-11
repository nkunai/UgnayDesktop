using UgnayDesktop.Services;

namespace UgnayDesktop.Forms;

public partial class TeacherDashboard
{
    private GestureRuntimeService? _gestureRuntimeService;
    private System.Windows.Forms.Timer? _gestureUiTimer;
    private GroupBox? _gestureGroup;
    private PictureBox? _gesturePreview;
    private Label? _gestureStatusLabel;
    private Label? _gesturePredictionLabel;
    private Label? _gestureModelLabel;
    private Label? _gestureMovementLabel;
    private Label? _gestureConfidenceLabel;
    private NumericUpDown? _cameraIndexInput;
    private Button? _startCameraButton;
    private Button? _stopCameraButton;
    private DateTime _lastPreviewWriteUtc = DateTime.MinValue;

    private void InitializeGestureStage3Ui()
    {
        ClientSize = new Size(Math.Max(ClientSize.Width, 1600), ClientSize.Height);
        MinimumSize = new Size(1500, 960);

        btnLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnTwilioConfigCheck.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        btnTwilioLink.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        btnTwilioTest.Anchor = AnchorStyles.Top | AnchorStyles.Left;

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

        var helperLabel = new Label
        {
            Location = new Point(18, 604),
            Size = new Size(350, 170),
            Text = "This panel uses the separated stage 3 models. Static labels stay in the static model, while moving gestures like j and z stay in the motion model. Use it to judge real-time behavior before the next data collection pass."
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

        if (_gesturePreview?.Image != null)
        {
            _gesturePreview.Image.Dispose();
            _gesturePreview.Image = null;
        }
    }
}
