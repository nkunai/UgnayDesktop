namespace UgnayDesktop.Forms;

public sealed class ForcePasswordChangeForm : Form
{
    private readonly Label _detailsLabel;
    private readonly TextBox _newPasswordInput;
    private readonly TextBox _confirmPasswordInput;

    public ForcePasswordChangeForm(string username)
    {
        Text = "Change Password";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(460, 260);

        var titleLabel = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            Location = new Point(20, 18),
            Text = "Set a new password"
        };

        _detailsLabel = new Label
        {
            AutoSize = true,
            Location = new Point(20, 50),
            Text = $"Account '{username}' must change its default password.",
        };

        var policyLabel = new Label
        {
            AutoSize = true,
            Location = new Point(20, 76),
            Text = "Use at least 8 characters with letters and numbers."
        };

        var newPasswordLabel = new Label
        {
            AutoSize = true,
            Location = new Point(20, 112),
            Text = "New password"
        };

        _newPasswordInput = new TextBox
        {
            Location = new Point(20, 132),
            Width = 410,
            UseSystemPasswordChar = true,
            MaxLength = 128,
        };

        var confirmLabel = new Label
        {
            AutoSize = true,
            Location = new Point(20, 164),
            Text = "Confirm password"
        };

        _confirmPasswordInput = new TextBox
        {
            Location = new Point(20, 184),
            Width = 410,
            UseSystemPasswordChar = true,
            MaxLength = 128,
        };

        var saveButton = new Button
        {
            Text = "Save",
            DialogResult = DialogResult.OK,
            Location = new Point(270, 220),
            Width = 76,
        };

        var cancelButton = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(354, 220),
            Width = 76,
        };

        AcceptButton = saveButton;
        CancelButton = cancelButton;

        Controls.Add(titleLabel);
        Controls.Add(_detailsLabel);
        Controls.Add(policyLabel);
        Controls.Add(newPasswordLabel);
        Controls.Add(_newPasswordInput);
        Controls.Add(confirmLabel);
        Controls.Add(_confirmPasswordInput);
        Controls.Add(saveButton);
        Controls.Add(cancelButton);
    }

    public string NewPassword => _newPasswordInput.Text;

    public string ConfirmPassword => _confirmPasswordInput.Text;
}

