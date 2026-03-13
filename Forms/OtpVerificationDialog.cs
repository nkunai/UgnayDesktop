using System.Drawing;
using UgnayDesktop.Services;

namespace UgnayDesktop.Forms;

internal sealed class OtpVerificationDialog : Form
{
    private readonly Func<string, Task<(bool Success, string Message)>> _verifyAsync;
    private readonly Func<Task<OtpChallengeResult>> _resendAsync;
    private readonly TextBox _otpTextBox;
    private readonly Label _statusLabel;
    private readonly Label _instructionLabel;
    private readonly Button _verifyButton;
    private readonly Button _resendButton;
    private readonly System.Windows.Forms.Timer _timer;

    private DateTime _expiresAtUtc;
    private DateTime _resendAvailableAtUtc;

    public OtpVerificationDialog(
        string title,
        string instruction,
        DateTime expiresAtUtc,
        DateTime resendAvailableAtUtc,
        Func<string, Task<(bool Success, string Message)>> verifyAsync,
        Func<Task<OtpChallengeResult>> resendAsync)
    {
        _verifyAsync = verifyAsync;
        _resendAsync = resendAsync;
        _expiresAtUtc = expiresAtUtc;
        _resendAvailableAtUtc = resendAvailableAtUtc;

        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(430, 230);

        _instructionLabel = new Label
        {
            AutoSize = false,
            Location = new Point(16, 18),
            Size = new Size(398, 52),
            Text = instruction
        };

        var otpLabel = new Label
        {
            Text = "OTP Code",
            AutoSize = true,
            Location = new Point(16, 86)
        };

        _otpTextBox = new TextBox
        {
            Location = new Point(16, 114),
            Size = new Size(180, 35),
            MaxLength = 6,
            TabIndex = 0
        };
        _otpTextBox.KeyPress += OtpTextBox_KeyPress;

        _statusLabel = new Label
        {
            AutoSize = false,
            ForeColor = Color.DimGray,
            Location = new Point(16, 156),
            Size = new Size(398, 36)
        };

        var cancelButton = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(150, 188),
            Size = new Size(82, 32),
            TabIndex = 3
        };

        _resendButton = new Button
        {
            Text = "Resend OTP",
            Location = new Point(238, 188),
            Size = new Size(82, 32),
            TabIndex = 2
        };
        _resendButton.Click += ResendButton_Click;

        _verifyButton = new Button
        {
            Text = "Verify",
            Location = new Point(326, 188),
            Size = new Size(88, 32),
            TabIndex = 1
        };
        _verifyButton.Click += VerifyButton_Click;

        Controls.Add(_instructionLabel);
        Controls.Add(otpLabel);
        Controls.Add(_otpTextBox);
        Controls.Add(_statusLabel);
        Controls.Add(cancelButton);
        Controls.Add(_resendButton);
        Controls.Add(_verifyButton);

        AcceptButton = _verifyButton;
        CancelButton = cancelButton;

        _timer = new System.Windows.Forms.Timer { Interval = 1000 };
        _timer.Tick += (_, _) => UpdateStatusText();
        _timer.Start();
        UpdateStatusText();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Dispose();
        }

        base.Dispose(disposing);
    }

    private void OtpTextBox_KeyPress(object? sender, KeyPressEventArgs e)
    {
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
        {
            e.Handled = true;
        }
    }

    private async void VerifyButton_Click(object? sender, EventArgs e)
    {
        var otpCode = _otpTextBox.Text.Trim();
        if (otpCode.Length != 6)
        {
            MessageBox.Show("OTP must be a 6-digit code.", "OTP", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _otpTextBox.Focus();
            return;
        }

        SetBusy(true);
        try
        {
            var result = await _verifyAsync(otpCode);
            if (!result.Success)
            {
                MessageBox.Show(result.Message, "OTP", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _otpTextBox.SelectAll();
                _otpTextBox.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void ResendButton_Click(object? sender, EventArgs e)
    {
        SetBusy(true);
        try
        {
            var result = await _resendAsync();
            if (!result.Success)
            {
                MessageBox.Show(result.Message, "OTP", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _expiresAtUtc = result.ExpiresAtUtc ?? DateTime.UtcNow;
            _resendAvailableAtUtc = result.ResendAvailableAtUtc ?? DateTime.UtcNow;
            _instructionLabel.Text = $"A new OTP was sent to {result.MaskedPhoneNumber ?? "your phone"}. Enter it below to continue.";
            _otpTextBox.Clear();
            _otpTextBox.Focus();
            UpdateStatusText();
            MessageBox.Show(result.Message, "OTP", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void UpdateStatusText()
    {
        var nowUtc = DateTime.UtcNow;
        var expiresIn = _expiresAtUtc - nowUtc;
        var resendIn = _resendAvailableAtUtc - nowUtc;

        var expiresText = expiresIn > TimeSpan.Zero
            ? $"Code expires in {expiresIn.Minutes:D2}:{expiresIn.Seconds:D2}."
            : "Code expired. Request a new OTP.";

        var resendText = resendIn > TimeSpan.Zero
            ? $" Resend available in {Math.Ceiling(resendIn.TotalSeconds):0}s."
            : " You can resend now.";

        _statusLabel.Text = expiresText + resendText;
        _resendButton.Enabled = resendIn <= TimeSpan.Zero;
    }

    private void SetBusy(bool busy)
    {
        _verifyButton.Enabled = !busy;
        _resendButton.Enabled = !busy && DateTime.UtcNow >= _resendAvailableAtUtc;
        _otpTextBox.Enabled = !busy;
        UseWaitCursor = busy;
    }
}
