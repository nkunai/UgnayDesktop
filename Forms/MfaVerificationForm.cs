namespace UgnayDesktop.Forms;

public sealed class MfaVerificationForm : Form
{
    private readonly Label _detailsLabel;
    private readonly TextBox _codeInput;

    public MfaVerificationForm(string maskedEmail, DateTime expiresAtLocal)
    {
        Text = "Email Verification";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(430, 205);

        var titleLabel = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            Location = new Point(20, 18),
            Text = "Enter verification code"
        };

        _detailsLabel = new Label
        {
            AutoSize = true,
            Location = new Point(20, 52),
            Text = $"A 6-digit code was sent to {maskedEmail}. Expires at {expiresAtLocal:hh:mm tt}."
        };

        _codeInput = new TextBox
        {
            Location = new Point(20, 88),
            Width = 385,
            MaxLength = 6
        };

        _codeInput.KeyPress += CodeInput_KeyPress;

        var verifyButton = new Button
        {
            Text = "Verify",
            DialogResult = DialogResult.OK,
            Location = new Point(244, 142),
            Width = 78
        };

        var cancelButton = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(327, 142),
            Width = 78
        };

        AcceptButton = verifyButton;
        CancelButton = cancelButton;

        Controls.Add(titleLabel);
        Controls.Add(_detailsLabel);
        Controls.Add(_codeInput);
        Controls.Add(verifyButton);
        Controls.Add(cancelButton);
    }

    public string VerificationCode => _codeInput.Text.Trim();

    private void CodeInput_KeyPress(object? sender, KeyPressEventArgs e)
    {
        if (char.IsControl(e.KeyChar))
            return;

        if (!char.IsDigit(e.KeyChar))
            e.Handled = true;
    }
}
