using UgnayDesktop.Models;
using UgnayDesktop.Services;

namespace UgnayDesktop.Forms;

public partial class LoginForm : Form
{
    private static readonly Color PageBackgroundColor = Color.FromArgb(245, 247, 250);
    private static readonly Color CardBackgroundColor = Color.White;
    private static readonly Color BorderColor = Color.FromArgb(221, 226, 232);
    private static readonly Color PrimaryTextColor = Color.FromArgb(24, 33, 45);
    private static readonly Color SecondaryTextColor = Color.FromArgb(90, 101, 115);
    private static readonly Color FieldBackgroundColor = Color.FromArgb(250, 251, 252);
    private static readonly Color FieldBorderColor = Color.FromArgb(208, 215, 224);
    private static readonly Color PrimaryButtonColor = Color.FromArgb(24, 33, 45);
    private static readonly Color StatusErrorColor = Color.FromArgb(163, 64, 50);
    private static readonly Color StatusInfoColor = Color.FromArgb(90, 101, 115);

    private bool _isPasswordVisible;

    public LoginForm()
    {
        InitializeComponent();
        StartPosition = FormStartPosition.CenterScreen;
        ConfigureUi();
        SetPasswordVisibility(false);
        ClearStatus();
        LoadLogo();
        txtUsername.Focus();
    }

    private void ConfigureUi()
    {
        BackColor = PageBackgroundColor;

        panelBrand.BackColor = Color.Transparent;
        panelLoginCard.BackColor = CardBackgroundColor;
        panelLoginCard.ForeColor = PrimaryTextColor;
        panelLoginCard.BorderStyle = BorderStyle.FixedSingle;

        lblBrandTitle.ForeColor = PrimaryTextColor;
        lblBrandBody.ForeColor = SecondaryTextColor;
        lblBrandFooter.ForeColor = SecondaryTextColor;
        lblCardEyebrow.ForeColor = SecondaryTextColor;
        lblCardTitle.ForeColor = PrimaryTextColor;
        lblCardBody.ForeColor = SecondaryTextColor;
        lblUsername.ForeColor = SecondaryTextColor;
        lblPassword.ForeColor = SecondaryTextColor;

        ConfigureTextBox(txtUsername, "Username");
        ConfigureTextBox(txtPassword, "Password");

        btnTogglePassword.BackColor = CardBackgroundColor;
        btnTogglePassword.ForeColor = SecondaryTextColor;
        btnTogglePassword.FlatAppearance.BorderColor = FieldBorderColor;
        btnTogglePassword.FlatAppearance.MouseDownBackColor = Color.FromArgb(238, 241, 245);
        btnTogglePassword.FlatAppearance.MouseOverBackColor = Color.FromArgb(244, 246, 248);
        btnTogglePassword.AccessibleName = "Show or hide password";

        btnLogin.BackColor = PrimaryButtonColor;
        btnLogin.ForeColor = Color.White;
        btnLogin.CornerRadius = 16;
        btnLogin.AccessibleName = "Login";

        btnCreateAccount.BackColor = CardBackgroundColor;
        btnCreateAccount.ForeColor = SecondaryTextColor;
        btnCreateAccount.FlatAppearance.BorderColor = BorderColor;
        btnCreateAccount.FlatAppearance.MouseDownBackColor = Color.FromArgb(238, 241, 245);
        btnCreateAccount.FlatAppearance.MouseOverBackColor = Color.FromArgb(244, 246, 248);
        btnCreateAccount.AccessibleName = "Create account";

        lblStatus.AutoEllipsis = true;
        lblStatus.ForeColor = SecondaryTextColor;
        lblStatus.Visible = false;

        txtUsername.AccessibleName = "Username";
        txtPassword.AccessibleName = "Password";
    }

    private void ConfigureTextBox(TextBox textBox, string accessibleName)
    {
        textBox.BackColor = FieldBackgroundColor;
        textBox.ForeColor = PrimaryTextColor;
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.AccessibleName = accessibleName;
    }

    private void SetPasswordVisibility(bool visible)
    {
        _isPasswordVisible = visible;
        txtPassword.UseSystemPasswordChar = !visible;
        btnTogglePassword.Text = visible ? "Hide" : "Show";
    }

    private void LoadLogo()
    {
        string logoPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Images", "UgnayLogo.png");
        if (!File.Exists(logoPath))
        {
            pictureBoxLogo.Visible = false;
            return;
        }

        using var stream = new FileStream(logoPath, FileMode.Open, FileAccess.Read);
        using var image = Image.FromStream(stream);
        pictureBoxLogo.Image?.Dispose();
        pictureBoxLogo.Image = new Bitmap(image);
        pictureBoxLogo.Visible = true;
    }

    private async void btnLogin_Click(object sender, EventArgs e)
    {
        if (!ValidateCredentials())
        {
            return;
        }

        SetLoginBusy(true);
        try
        {
            var auth = new AuthService();
            var result = await auth.BeginLoginAsync(txtUsername.Text.Trim(), txtPassword.Text);

            if (result.Status == AuthLoginStatus.InvalidCredentials)
            {
                ShowStatus(result.Message, true);
                txtPassword.Focus();
                txtPassword.SelectAll();
                return;
            }

            if (result.Status == AuthLoginStatus.Blocked || result.Status == AuthLoginStatus.Error)
            {
                ShowStatus(result.Message, true);
                txtUsername.Focus();
                txtUsername.SelectAll();
                return;
            }

            if (result.Status == AuthLoginStatus.Authenticated && result.User != null)
            {
                ClearStatus();
                OpenDashboard(result.User);
                return;
            }

            if (result.Status == AuthLoginStatus.OtpRequired && result.ChallengeId != null && result.ExpiresAtUtc != null && result.ResendAvailableAtUtc != null)
            {
                ClearStatus();
                User? authenticatedTeacher = null;
                using var otpDialog = new OtpVerificationDialog(
                    "Teacher Sign-In OTP",
                    $"We sent an OTP to {result.MaskedPhoneNumber ?? "your phone"}. Enter it below to finish signing in.",
                    result.ExpiresAtUtc.Value,
                    result.ResendAvailableAtUtc.Value,
                    code =>
                    {
                        var complete = auth.CompleteTeacherLoginOtp(result.ChallengeId.Value, code);
                        if (complete.Success)
                        {
                            authenticatedTeacher = complete.User;
                        }

                        return Task.FromResult((complete.Success, complete.Message));
                    },
                    () => auth.ResendTeacherLoginOtpAsync(result.ChallengeId.Value));

                if (otpDialog.ShowDialog(this) == DialogResult.OK && authenticatedTeacher != null)
                {
                    OpenDashboard(authenticatedTeacher);
                }

                return;
            }

            ShowStatus("Login could not continue.", true);
        }
        finally
        {
            SetLoginBusy(false);
        }
    }

    private bool ValidateCredentials()
    {
        var username = txtUsername.Text.Trim();
        var password = txtPassword.Text;

        if (string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(password))
        {
            ShowStatus("Enter your username and password to continue.", true);
            txtUsername.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            ShowStatus("Enter your username.", true);
            txtUsername.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            ShowStatus("Enter your password.", true);
            txtPassword.Focus();
            return false;
        }

        return true;
    }

    private void OpenDashboard(User user)
    {
        Form dashboard;

        if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            dashboard = new AdminDashboard(user);
        }
        else if (string.Equals(user.Role, "Teacher", StringComparison.OrdinalIgnoreCase))
        {
            dashboard = new TeacherDashboard(user);
        }
        else
        {
            MessageBox.Show(
                "Student accounts are for ESP32 assignment and cannot sign in to the desktop dashboard.",
                "Access Restricted",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        dashboard.FormClosed += Dashboard_FormClosed;
        dashboard.Show();
        Hide();
    }

    private void Dashboard_FormClosed(object? sender, FormClosedEventArgs e)
    {
        txtUsername.Clear();
        txtPassword.Clear();
        SetPasswordVisibility(false);
        ClearStatus();
        SetLoginBusy(false);
        Show();
        Activate();
        txtUsername.Focus();
    }

    private void btnTogglePassword_Click(object sender, EventArgs e)
    {
        SetPasswordVisibility(!_isPasswordVisible);
    }

    private void btnCreateAccount_Click(object sender, EventArgs e)
    {
        using var dialog = new TeacherCreateAccountDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        txtUsername.Text = dialog.CreatedUsername;
        txtPassword.Clear();
        ClearStatus();
        txtUsername.Focus();
        SetPasswordVisibility(false);
    }

    private void txtInput_TextChanged(object sender, EventArgs e)
    {
        ClearStatus();
    }

    private void LoginForm_Load(object sender, EventArgs e)
    {
    }

    private void ShowStatus(string message, bool isError)
    {
        lblStatus.Text = message;
        lblStatus.ForeColor = isError ? StatusErrorColor : StatusInfoColor;
        lblStatus.Visible = true;
    }

    private void ClearStatus()
    {
        lblStatus.Text = string.Empty;
        lblStatus.Visible = false;
    }

    private void SetLoginBusy(bool busy)
    {
        btnLogin.Enabled = !busy;
        btnCreateAccount.Enabled = !busy;
        txtUsername.Enabled = !busy;
        txtPassword.Enabled = !busy;
        btnTogglePassword.Enabled = !busy;
        btnLogin.Text = busy ? "Signing In..." : "Login";

        if (busy)
        {
            ShowStatus("Checking your credentials...", false);
        }

        UseWaitCursor = busy;
    }
}
