using System.Drawing;
using UgnayDesktop.Services;

namespace UgnayDesktop.Forms;

internal sealed class TeacherCreateAccountDialog : Form
{
    private readonly TextBox _fullNameTextBox;
    private readonly TextBox _usernameTextBox;
    private readonly TextBox _passwordTextBox;
    private readonly TextBox _confirmPasswordTextBox;
    private readonly TextBox _contactDigitsTextBox;
    private readonly Button _createButton;

    public string CreatedUsername { get; private set; } = string.Empty;

    public TeacherCreateAccountDialog()
    {
        Text = "Create Teacher Account";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(460, 390);

        var fullNameLabel = new Label
        {
            Text = "Full Name",
            AutoSize = true,
            Location = new Point(16, 18)
        };

        _fullNameTextBox = new TextBox
        {
            Location = new Point(16, 46),
            Size = new Size(426, 35),
            TabIndex = 0
        };

        var usernameLabel = new Label
        {
            Text = "Username",
            AutoSize = true,
            Location = new Point(16, 92)
        };

        _usernameTextBox = new TextBox
        {
            Location = new Point(16, 120),
            Size = new Size(426, 35),
            TabIndex = 1
        };

        var passwordLabel = new Label
        {
            Text = "Password",
            AutoSize = true,
            Location = new Point(16, 166)
        };

        _passwordTextBox = new TextBox
        {
            Location = new Point(16, 194),
            Size = new Size(426, 35),
            TabIndex = 2,
            UseSystemPasswordChar = true
        };

        var confirmPasswordLabel = new Label
        {
            Text = "Confirm Password",
            AutoSize = true,
            Location = new Point(16, 240)
        };

        _confirmPasswordTextBox = new TextBox
        {
            Location = new Point(16, 268),
            Size = new Size(426, 35),
            TabIndex = 3,
            UseSystemPasswordChar = true
        };

        var contactLabel = new Label
        {
            Text = "Contact Number",
            AutoSize = true,
            Location = new Point(16, 314)
        };

        var prefixLabel = new Label
        {
            Text = "+63",
            AutoSize = true,
            Location = new Point(16, 346)
        };

        _contactDigitsTextBox = new TextBox
        {
            Location = new Point(62, 342),
            Size = new Size(180, 35),
            MaxLength = 10,
            TabIndex = 4
        };
        _contactDigitsTextBox.KeyPress += ContactDigitsTextBox_KeyPress;

        var hintLabel = new Label
        {
            Text = "Teacher sign-up only. We will verify this number with OTP.",
            AutoSize = true,
            ForeColor = Color.DimGray,
            Location = new Point(248, 346)
        };

        var cancelButton = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(260, 18),
            Size = new Size(86, 34),
            TabIndex = 6
        };

        _createButton = new Button
        {
            Text = "Send OTP",
            Location = new Point(352, 18),
            Size = new Size(90, 34),
            TabIndex = 5
        };
        _createButton.Click += CreateButton_Click;

        Controls.Add(fullNameLabel);
        Controls.Add(_fullNameTextBox);
        Controls.Add(usernameLabel);
        Controls.Add(_usernameTextBox);
        Controls.Add(passwordLabel);
        Controls.Add(_passwordTextBox);
        Controls.Add(confirmPasswordLabel);
        Controls.Add(_confirmPasswordTextBox);
        Controls.Add(contactLabel);
        Controls.Add(prefixLabel);
        Controls.Add(_contactDigitsTextBox);
        Controls.Add(hintLabel);
        Controls.Add(cancelButton);
        Controls.Add(_createButton);

        AcceptButton = _createButton;
        CancelButton = cancelButton;
    }

    private void ContactDigitsTextBox_KeyPress(object? sender, KeyPressEventArgs e)
    {
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
        {
            e.Handled = true;
        }
    }

    private async void CreateButton_Click(object? sender, EventArgs e)
    {
        var auth = new AuthService();
        SetBusy(true);

        try
        {
            var beginResult = await auth.BeginTeacherSignupAsync(
                _fullNameTextBox.Text,
                _usernameTextBox.Text,
                _passwordTextBox.Text,
                _confirmPasswordTextBox.Text,
                _contactDigitsTextBox.Text);

            if (!beginResult.Success || beginResult.ChallengeId == null || beginResult.ExpiresAtUtc == null || beginResult.ResendAvailableAtUtc == null)
            {
                MessageBox.Show(beginResult.Message, "Create Account", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var createdUsername = string.Empty;
            using var otpDialog = new OtpVerificationDialog(
                "Verify Teacher Phone",
                $"We sent an OTP to {beginResult.MaskedPhoneNumber ?? "your phone"}. Enter it below to finish creating your teacher account.",
                beginResult.ExpiresAtUtc.Value,
                beginResult.ResendAvailableAtUtc.Value,
                code =>
                {
                    var complete = auth.CompleteTeacherSignup(beginResult.ChallengeId.Value, code);
                    if (complete.Success)
                    {
                        createdUsername = complete.Username ?? _usernameTextBox.Text.Trim();
                    }

                    return Task.FromResult((complete.Success, complete.Message));
                },
                () => auth.ResendTeacherSignupOtpAsync(beginResult.ChallengeId.Value));

            if (otpDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            CreatedUsername = createdUsername;
            DialogResult = DialogResult.OK;
            Close();
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool busy)
    {
        _createButton.Enabled = !busy;
        UseWaitCursor = busy;
    }
}
