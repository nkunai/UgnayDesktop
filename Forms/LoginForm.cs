using UgnayDesktop.Services;
using UgnayDesktop.Models;
using System.Drawing.Drawing2D;

namespace UgnayDesktop.Forms;

public partial class LoginForm : Form
{
    private readonly AuthService _authService;
    private readonly EmailMfaService _mfaService;

    public LoginForm()
    {
        InitializeComponent();
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = ColorTranslator.FromHtml("#CABA9C");

        _authService = new AuthService();
        _mfaService = new EmailMfaService(new LocalEmailVerificationSender());

        label1.ForeColor = ColorTranslator.FromHtml("#545454");
        label2.ForeColor = ColorTranslator.FromHtml("#545454");

        txtUsername.BackColor = ColorTranslator.FromHtml("#545454");
        txtPassword.BackColor = ColorTranslator.FromHtml("#545454");
        txtUsername.ForeColor = Color.White;
        txtPassword.ForeColor = Color.White;
        txtUsername.BorderStyle = BorderStyle.None;
        txtPassword.BorderStyle = BorderStyle.None;
        RoundTextBox(txtUsername, 12);
        RoundTextBox(txtPassword, 12);
        txtUsername.Resize += (_, _) => RoundTextBox(txtUsername, 12);
        txtPassword.Resize += (_, _) => RoundTextBox(txtPassword, 12);

        btnLogin.BackColor = ColorTranslator.FromHtml("#D9D9D9");
        btnLogin.ForeColor = Color.Black;
        btnLogin.CornerRadius = 18;

        LoadLogo();
    }

    private void LoadLogo()
    {
        string logoPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Images", "UgnayLogo.png");
        if (!File.Exists(logoPath))
        {
            return;
        }

        using var stream = new FileStream(logoPath, FileMode.Open, FileAccess.Read);
        using var image = Image.FromStream(stream);
        pictureBoxLogo.Image?.Dispose();
        pictureBoxLogo.Image = new Bitmap(image);
    }

    private void label1_Click(object sender, EventArgs e)
    {

    }

    private void label3_Click(object sender, EventArgs e)
    {

    }

    private void btnLogin_Click(object sender, EventArgs e)
    {
        var user = _authService.ValidateCredentials(txtUsername.Text.Trim(), txtPassword.Text.Trim());

        if (user == null)
        {
            MessageBox.Show("Invalid username or password", "Login Failed",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!CompleteMfaIfRequired(user))
            return;

        if (!CompleteMandatoryPasswordChangeIfRequired(user))
            return;

        OpenDashboard(user);
    }

    private bool CompleteMfaIfRequired(User user)
    {
        if (!_mfaService.IsMfaRequired(user))
            return true;

        var challenge = _mfaService.BeginChallenge(user);

        if (!challenge.IsSuccess)
        {
            MessageBox.Show(
                challenge.ErrorMessage + " Please contact an admin to set your email before MFA login.",
                "MFA Not Configured",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return false;
        }

        MessageBox.Show(
            "Verification code sent. During development, codes are also written to mfa-email-preview.log in the app folder.",
            "Email Verification",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        while (true)
        {
            using var verifyForm = new MfaVerificationForm(challenge.MaskedEmail, challenge.ExpiresAtUtc.ToLocalTime());

            if (verifyForm.ShowDialog(this) != DialogResult.OK)
                return false;

            var verifyResult = _mfaService.VerifyCode(challenge.ChallengeId, user.Id, verifyForm.VerificationCode);
            if (verifyResult.IsSuccess)
                return true;

            if (verifyResult.IsExpired)
            {
                var resend = MessageBox.Show(
                    "Your verification code has expired. Send a new one?",
                    "Code Expired",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (resend != DialogResult.Yes)
                    return false;

                challenge = _mfaService.BeginChallenge(user);
                if (!challenge.IsSuccess)
                {
                    MessageBox.Show(challenge.ErrorMessage, "MFA Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                continue;
            }

            MessageBox.Show(verifyResult.ErrorMessage, "Verification Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }


    private bool CompleteMandatoryPasswordChangeIfRequired(User user)
    {
        if (!user.MustChangePassword)
            return true;

        MessageBox.Show(
            "You must change your default password before continuing.",
            "Password Update Required",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        while (true)
        {
            using var changePasswordForm = new ForcePasswordChangeForm(user.Username);
            if (changePasswordForm.ShowDialog(this) != DialogResult.OK)
                return false;

            if (!string.Equals(changePasswordForm.NewPassword, changePasswordForm.ConfirmPassword, StringComparison.Ordinal))
            {
                MessageBox.Show("Passwords do not match.", "Password Update", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                continue;
            }

            if (!_authService.TrySetNewPassword(user.Id, changePasswordForm.NewPassword, out var errorMessage))
            {
                MessageBox.Show(errorMessage, "Password Update", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                continue;
            }

            user.MustChangePassword = false;
            MessageBox.Show("Password updated successfully.", "Password Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }
    }
    private void OpenDashboard(User user)
    {
        Form dashboard;

        if (user.Role == "Admin")
        {
            dashboard = new AdminDashboard(user);
        }
        else if (user.Role == "Teacher")
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
        Show();
        Activate();
    }

    private void txtPassword_TextChanged(object sender, EventArgs e)
    {

    }

    private void LoginForm_Load(object sender, EventArgs e)
    {

    }

    private static void RoundTextBox(TextBox textBox, int radius)
    {
        var path = new GraphicsPath();
        int diameter = radius * 2;

        path.AddArc(0, 0, diameter, diameter, 180, 90);
        path.AddArc(textBox.Width - diameter, 0, diameter, diameter, 270, 90);
        path.AddArc(textBox.Width - diameter, textBox.Height - diameter, diameter, diameter, 0, 90);
        path.AddArc(0, textBox.Height - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        textBox.Region?.Dispose();
        textBox.Region = new Region(path);
    }

    private void label2_Click(object sender, EventArgs e)
    {

    }
}




