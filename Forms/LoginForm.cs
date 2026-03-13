using UgnayDesktop.Services;
using UgnayDesktop.Models;
using System.Drawing.Drawing2D;

namespace UgnayDesktop.Forms;

public partial class LoginForm : Form
{
    private bool _isPasswordVisible;

    public LoginForm()
    {
        InitializeComponent();
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.White;

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

        btnTogglePassword.BackColor = ColorTranslator.FromHtml("#D9D9D9");
        btnTogglePassword.ForeColor = Color.Black;

        btnLogin.BackColor = ColorTranslator.FromHtml("#D9D9D9");
        btnLogin.ForeColor = Color.Black;
        btnLogin.CornerRadius = 18;

        btnCreateAccount.BackColor = Color.White;
        btnCreateAccount.ForeColor = ColorTranslator.FromHtml("#545454");

        SetPasswordVisibility(false);
        LoadLogo();
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

    private async void btnLogin_Click(object sender, EventArgs e)
    {
        SetLoginBusy(true);
        try
        {
            var auth = new AuthService();
            var result = await auth.BeginLoginAsync(txtUsername.Text, txtPassword.Text);

            if (result.Status == AuthLoginStatus.InvalidCredentials || result.Status == AuthLoginStatus.Blocked || result.Status == AuthLoginStatus.Error)
            {
                MessageBox.Show(result.Message, "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (result.Status == AuthLoginStatus.Authenticated && result.User != null)
            {
                OpenDashboard(result.User);
                return;
            }

            if (result.Status == AuthLoginStatus.OtpRequired && result.ChallengeId != null && result.ExpiresAtUtc != null && result.ResendAvailableAtUtc != null)
            {
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

            MessageBox.Show("Login could not continue.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            SetLoginBusy(false);
        }
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
        Show();
        Activate();
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
        txtUsername.Focus();
        SetPasswordVisibility(false);
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

    private void SetLoginBusy(bool busy)
    {
        btnLogin.Enabled = !busy;
        btnCreateAccount.Enabled = !busy;
        txtUsername.Enabled = !busy;
        txtPassword.Enabled = !busy;
        btnTogglePassword.Enabled = !busy;
        UseWaitCursor = busy;
    }
}
