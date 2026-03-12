namespace UgnayDesktop.Forms;

partial class LoginForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        label1 = new Label();
        txtUsername = new TextBox();
        label2 = new Label();
        txtPassword = new TextBox();
        btnTogglePassword = new Button();
        btnLogin = new UgnayDesktop.Controls.RoundedButton();
        label3 = new Label();
        pictureBoxLogo = new PictureBox();
        ((System.ComponentModel.ISupportInitialize)pictureBoxLogo).BeginInit();
        SuspendLayout();
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new Point(948, 239);
        label1.Name = "label1";
        label1.Size = new Size(106, 30);
        label1.TabIndex = 0;
        label1.Text = "Username";
        label1.Click += label1_Click;
        // 
        // txtUsername
        // 
        txtUsername.Font = new Font("Segoe UI", 11F);
        txtUsername.Location = new Point(878, 272);
        txtUsername.Multiline = true;
        txtUsername.Name = "txtUsername";
        txtUsername.Size = new Size(260, 45);
        txtUsername.TabIndex = 1;
        txtUsername.Tag = "";
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.Location = new Point(955, 360);
        label2.Name = "label2";
        label2.Size = new Size(99, 30);
        label2.TabIndex = 2;
        label2.Text = "Password";
        label2.Click += label2_Click;
        // 
        // txtPassword
        // 
        txtPassword.Font = new Font("Segoe UI", 11F);
        txtPassword.Location = new Point(878, 393);
        txtPassword.Multiline = false;
        txtPassword.Name = "txtPassword";
        txtPassword.AutoSize = false;
        txtPassword.Size = new Size(260, 45);
        txtPassword.TabIndex = 3;
        txtPassword.UseSystemPasswordChar = true;
        txtPassword.TextChanged += txtPassword_TextChanged;
        // 
        // btnTogglePassword
        // 
        btnTogglePassword.BackColor = Color.FromArgb(217, 217, 217);
        btnTogglePassword.FlatStyle = FlatStyle.Flat;
        btnTogglePassword.ForeColor = Color.Black;
        btnTogglePassword.Location = new Point(1144, 393);
        btnTogglePassword.Name = "btnTogglePassword";
        btnTogglePassword.Size = new Size(70, 45);
        btnTogglePassword.TabIndex = 4;
        btnTogglePassword.Text = "Show";
        btnTogglePassword.UseVisualStyleBackColor = false;
        btnTogglePassword.Click += btnTogglePassword_Click;
        // 
        // btnLogin
        // 
        btnLogin.BackColor = Color.FromArgb(217, 217, 217);
        btnLogin.FlatStyle = FlatStyle.Flat;
        btnLogin.ForeColor = Color.Black;
        btnLogin.Location = new Point(939, 464);
        btnLogin.Name = "btnLogin";
        btnLogin.Size = new Size(131, 40);
        btnLogin.TabIndex = 5;
        btnLogin.Text = "Login";
        btnLogin.UseVisualStyleBackColor = true;
        btnLogin.Click += btnLogin_Click;
        // 
        // label3
        // 
        label3.Font = new Font("Arial Rounded MT Bold", 35F, FontStyle.Bold);
        label3.Location = new Point(838, 93);
        label3.Name = "label3";
        label3.Size = new Size(349, 120);
        label3.TabIndex = 6;
        label3.Text = "UGNAY";
        label3.Click += label3_Click;
        // 
        // pictureBoxLogo
        // 
        pictureBoxLogo.BackColor = Color.Transparent;
        pictureBoxLogo.Location = new Point(86, 93);
        pictureBoxLogo.Name = "pictureBoxLogo";
        pictureBoxLogo.Size = new Size(680, 540);
        pictureBoxLogo.SizeMode = PictureBoxSizeMode.Zoom;
        pictureBoxLogo.TabIndex = 7;
        pictureBoxLogo.TabStop = false;
        // 
        // LoginForm
        // 
        AcceptButton = btnLogin;
        AutoScaleDimensions = new SizeF(12F, 30F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.CornflowerBlue;
        ClientSize = new Size(1243, 701);
        Controls.Add(btnTogglePassword);
        Controls.Add(pictureBoxLogo);
        Controls.Add(label3);
        Controls.Add(btnLogin);
        Controls.Add(txtPassword);
        Controls.Add(label2);
        Controls.Add(txtUsername);
        Controls.Add(label1);
        Name = "LoginForm";
        Text = "UGNAY Login";
        Load += LoginForm_Load;
        ((System.ComponentModel.ISupportInitialize)pictureBoxLogo).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label label1;
    private TextBox txtUsername;
    private Label label2;
    private TextBox txtPassword;
    private Button btnTogglePassword;
    private UgnayDesktop.Controls.RoundedButton btnLogin;
    private Label label3;
    private PictureBox pictureBoxLogo;
}


