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

    private void btnLogin_Click(object sender, EventArgs e)
    {
        var username = txtUsername.Text.Trim();
        var password = txtPassword.Text.Trim();
        var auth = new AuthService();
        var user = auth.Login(username, password);

        if (user == null)
        {
            MessageBox.Show("Invalid username or password", "Login Failed",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

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
        SetPasswordVisibility(false);
        Show();
        Activate();
    }

    private void btnTogglePassword_Click(object sender, EventArgs e)
    {
        SetPasswordVisibility(!_isPasswordVisible);
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


