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
    /// <param name="disposing">true if managed resources are being disposed; otherwise, false.</param>
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
        tableLayoutRoot = new TableLayoutPanel();
        panelBrand = new Panel();
        lblBrandFooter = new Label();
        lblBrandBody = new Label();
        lblBrandTitle = new Label();
        pictureBoxLogo = new PictureBox();
        panelLoginCard = new Panel();
        layoutLoginCard = new TableLayoutPanel();
        lblCardEyebrow = new Label();
        lblCardTitle = new Label();
        lblCardBody = new Label();
        lblUsername = new Label();
        txtUsername = new TextBox();
        lblPassword = new Label();
        panelPasswordRow = new Panel();
        btnTogglePassword = new Button();
        txtPassword = new TextBox();
        lblStatus = new Label();
        btnLogin = new UgnayDesktop.Controls.RoundedButton();
        btnCreateAccount = new Button();
        tableLayoutRoot.SuspendLayout();
        panelBrand.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)pictureBoxLogo).BeginInit();
        panelLoginCard.SuspendLayout();
        layoutLoginCard.SuspendLayout();
        panelPasswordRow.SuspendLayout();
        SuspendLayout();
        // 
        // tableLayoutRoot
        // 
        tableLayoutRoot.ColumnCount = 2;
        tableLayoutRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 53F));
        tableLayoutRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 47F));
        tableLayoutRoot.Controls.Add(panelBrand, 0, 0);
        tableLayoutRoot.Controls.Add(panelLoginCard, 1, 0);
        tableLayoutRoot.Dock = DockStyle.Fill;
        tableLayoutRoot.Location = new Point(0, 0);
        tableLayoutRoot.Name = "tableLayoutRoot";
        tableLayoutRoot.Padding = new Padding(24);
        tableLayoutRoot.RowCount = 1;
        tableLayoutRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tableLayoutRoot.Size = new Size(1243, 701);
        tableLayoutRoot.TabIndex = 0;
        // 
        // panelBrand
        // 
        panelBrand.Controls.Add(lblBrandFooter);
        panelBrand.Controls.Add(lblBrandBody);
        panelBrand.Controls.Add(lblBrandTitle);
        panelBrand.Controls.Add(pictureBoxLogo);
        panelBrand.Dock = DockStyle.Fill;
        panelBrand.Location = new Point(27, 27);
        panelBrand.Name = "panelBrand";
        panelBrand.Padding = new Padding(20);
        panelBrand.Size = new Size(627, 647);
        panelBrand.TabIndex = 0;
        // 
        // lblBrandFooter
        // 
        lblBrandFooter.AutoSize = true;
        lblBrandFooter.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
        lblBrandFooter.Location = new Point(20, 583);
        lblBrandFooter.Name = "lblBrandFooter";
        lblBrandFooter.Size = new Size(174, 25);
        lblBrandFooter.TabIndex = 3;
        lblBrandFooter.Text = "Desktop sign-in portal";
        // 
        // lblBrandBody
        // 
        lblBrandBody.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point, 0);
        lblBrandBody.Location = new Point(20, 519);
        lblBrandBody.Name = "lblBrandBody";
        lblBrandBody.Size = new Size(470, 60);
        lblBrandBody.TabIndex = 2;
        lblBrandBody.Text = "Monitor students, track sensor activity, and respond quickly from one dashboard.";
        // 
        // lblBrandTitle
        // 
        lblBrandTitle.AutoSize = true;
        lblBrandTitle.Font = new Font("Segoe UI", 24F, FontStyle.Bold, GraphicsUnit.Point, 0);
        lblBrandTitle.Location = new Point(20, 448);
        lblBrandTitle.Name = "lblBrandTitle";
        lblBrandTitle.Size = new Size(183, 65);
        lblBrandTitle.TabIndex = 1;
        lblBrandTitle.Text = "UGNAY";
        // 
        // pictureBoxLogo
        // 
        pictureBoxLogo.BackColor = Color.Transparent;
        pictureBoxLogo.Location = new Point(20, 44);
        pictureBoxLogo.Name = "pictureBoxLogo";
        pictureBoxLogo.Size = new Size(520, 360);
        pictureBoxLogo.SizeMode = PictureBoxSizeMode.Zoom;
        pictureBoxLogo.TabIndex = 0;
        pictureBoxLogo.TabStop = false;
        // 
        // panelLoginCard
        // 
        panelLoginCard.Anchor = AnchorStyles.None;
        panelLoginCard.BorderStyle = BorderStyle.FixedSingle;
        panelLoginCard.Controls.Add(layoutLoginCard);
        panelLoginCard.Location = new Point(711, 88);
        panelLoginCard.Name = "panelLoginCard";
        panelLoginCard.Padding = new Padding(28);
        panelLoginCard.Size = new Size(481, 525);
        panelLoginCard.TabIndex = 1;
        // 
        // layoutLoginCard
        // 
        layoutLoginCard.ColumnCount = 1;
        layoutLoginCard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutLoginCard.Controls.Add(lblCardEyebrow, 0, 0);
        layoutLoginCard.Controls.Add(lblCardTitle, 0, 1);
        layoutLoginCard.Controls.Add(lblCardBody, 0, 2);
        layoutLoginCard.Controls.Add(lblUsername, 0, 3);
        layoutLoginCard.Controls.Add(txtUsername, 0, 4);
        layoutLoginCard.Controls.Add(lblPassword, 0, 5);
        layoutLoginCard.Controls.Add(panelPasswordRow, 0, 6);
        layoutLoginCard.Controls.Add(lblStatus, 0, 7);
        layoutLoginCard.Controls.Add(btnLogin, 0, 8);
        layoutLoginCard.Controls.Add(btnCreateAccount, 0, 9);
        layoutLoginCard.Dock = DockStyle.Fill;
        layoutLoginCard.Location = new Point(28, 28);
        layoutLoginCard.Name = "layoutLoginCard";
        layoutLoginCard.RowCount = 10;
        layoutLoginCard.RowStyles.Add(new RowStyle());
        layoutLoginCard.RowStyles.Add(new RowStyle());
        layoutLoginCard.RowStyles.Add(new RowStyle());
        layoutLoginCard.RowStyles.Add(new RowStyle());
        layoutLoginCard.RowStyles.Add(new RowStyle());
        layoutLoginCard.RowStyles.Add(new RowStyle());
        layoutLoginCard.RowStyles.Add(new RowStyle());
        layoutLoginCard.RowStyles.Add(new RowStyle());
        layoutLoginCard.RowStyles.Add(new RowStyle());
        layoutLoginCard.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layoutLoginCard.Size = new Size(423, 467);
        layoutLoginCard.TabIndex = 0;
        // 
        // lblCardEyebrow
        // 
        lblCardEyebrow.AutoSize = true;
        lblCardEyebrow.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
        lblCardEyebrow.Location = new Point(3, 0);
        lblCardEyebrow.Margin = new Padding(3, 0, 3, 6);
        lblCardEyebrow.Name = "lblCardEyebrow";
        lblCardEyebrow.Size = new Size(116, 25);
        lblCardEyebrow.TabIndex = 0;
        lblCardEyebrow.Text = "Teacher portal";
        // 
        // lblCardTitle
        // 
        lblCardTitle.AutoSize = true;
        lblCardTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Point, 0);
        lblCardTitle.Location = new Point(3, 31);
        lblCardTitle.Margin = new Padding(3, 0, 3, 8);
        lblCardTitle.Name = "lblCardTitle";
        lblCardTitle.Size = new Size(184, 54);
        lblCardTitle.TabIndex = 1;
        lblCardTitle.Text = "Welcome";
        // 
        // lblCardBody
        // 
        lblCardBody.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
        lblCardBody.Location = new Point(3, 93);
        lblCardBody.Margin = new Padding(3, 0, 3, 28);
        lblCardBody.Name = "lblCardBody";
        lblCardBody.Size = new Size(404, 48);
        lblCardBody.TabIndex = 2;
        lblCardBody.Text = "Sign in with your UGNAY account to access the dashboard and alerts.";
        // 
        // lblUsername
        // 
        lblUsername.AutoSize = true;
        lblUsername.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
        lblUsername.Location = new Point(3, 169);
        lblUsername.Margin = new Padding(3, 0, 3, 8);
        lblUsername.Name = "lblUsername";
        lblUsername.Size = new Size(90, 25);
        lblUsername.TabIndex = 3;
        lblUsername.Text = "Username";
        // 
        // txtUsername
        // 
        txtUsername.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        txtUsername.BorderStyle = BorderStyle.FixedSingle;
        txtUsername.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point, 0);
        txtUsername.Location = new Point(3, 202);
        txtUsername.Margin = new Padding(3, 0, 3, 20);
        txtUsername.Name = "txtUsername";
        txtUsername.PlaceholderText = "Enter your username";
        txtUsername.Size = new Size(417, 37);
        txtUsername.TabIndex = 1;
        txtUsername.TextChanged += txtInput_TextChanged;
        // 
        // lblPassword
        // 
        lblPassword.AutoSize = true;
        lblPassword.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
        lblPassword.Location = new Point(3, 259);
        lblPassword.Margin = new Padding(3, 0, 3, 8);
        lblPassword.Name = "lblPassword";
        lblPassword.Size = new Size(83, 25);
        lblPassword.TabIndex = 5;
        lblPassword.Text = "Password";
        // 
        // panelPasswordRow
        // 
        panelPasswordRow.Controls.Add(btnTogglePassword);
        panelPasswordRow.Controls.Add(txtPassword);
        panelPasswordRow.Dock = DockStyle.Top;
        panelPasswordRow.Location = new Point(3, 292);
        panelPasswordRow.Margin = new Padding(3, 0, 3, 20);
        panelPasswordRow.Name = "panelPasswordRow";
        panelPasswordRow.Size = new Size(417, 38);
        panelPasswordRow.TabIndex = 6;
        // 
        // btnTogglePassword
        // 
        btnTogglePassword.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnTogglePassword.FlatStyle = FlatStyle.Flat;
        btnTogglePassword.Location = new Point(321, 0);
        btnTogglePassword.Name = "btnTogglePassword";
        btnTogglePassword.Size = new Size(96, 38);
        btnTogglePassword.TabIndex = 3;
        btnTogglePassword.Text = "Show";
        btnTogglePassword.UseVisualStyleBackColor = true;
        btnTogglePassword.Click += btnTogglePassword_Click;
        // 
        // txtPassword
        // 
        txtPassword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtPassword.BorderStyle = BorderStyle.FixedSingle;
        txtPassword.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point, 0);
        txtPassword.Location = new Point(0, 0);
        txtPassword.Name = "txtPassword";
        txtPassword.PlaceholderText = "Enter your password";
        txtPassword.Size = new Size(309, 37);
        txtPassword.TabIndex = 2;
        txtPassword.UseSystemPasswordChar = true;
        txtPassword.TextChanged += txtInput_TextChanged;
        // 
        // lblStatus
        // 
        lblStatus.Dock = DockStyle.Top;
        lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
        lblStatus.Location = new Point(3, 350);
        lblStatus.Margin = new Padding(3, 0, 3, 16);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(417, 44);
        lblStatus.TabIndex = 7;
        lblStatus.Text = "Status message";
        // 
        // btnLogin
        // 
        btnLogin.BackColor = Color.FromArgb(24, 33, 45);
        btnLogin.CornerRadius = 16;
        btnLogin.Dock = DockStyle.Top;
        btnLogin.FlatStyle = FlatStyle.Flat;
        btnLogin.ForeColor = Color.White;
        btnLogin.Location = new Point(3, 410);
        btnLogin.Margin = new Padding(3, 0, 3, 12);
        btnLogin.Name = "btnLogin";
        btnLogin.Size = new Size(417, 46);
        btnLogin.TabIndex = 4;
        btnLogin.Text = "Login";
        btnLogin.UseVisualStyleBackColor = false;
        btnLogin.Click += btnLogin_Click;
        // 
        // btnCreateAccount
        // 
        btnCreateAccount.Anchor = AnchorStyles.Top;
        btnCreateAccount.FlatStyle = FlatStyle.Flat;
        btnCreateAccount.Location = new Point(124, 471);
        btnCreateAccount.Name = "btnCreateAccount";
        btnCreateAccount.Size = new Size(174, 38);
        btnCreateAccount.TabIndex = 5;
        btnCreateAccount.Text = "Create Account";
        btnCreateAccount.UseVisualStyleBackColor = true;
        btnCreateAccount.Click += btnCreateAccount_Click;
        // 
        // LoginForm
        // 
        AcceptButton = btnLogin;
        AutoScaleDimensions = new SizeF(10F, 25F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(245, 247, 250);
        ClientSize = new Size(1243, 701);
        Controls.Add(tableLayoutRoot);
        MinimumSize = new Size(1100, 720);
        Name = "LoginForm";
        Text = "UGNAY Login";
        Load += LoginForm_Load;
        tableLayoutRoot.ResumeLayout(false);
        panelBrand.ResumeLayout(false);
        panelBrand.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)pictureBoxLogo).EndInit();
        panelLoginCard.ResumeLayout(false);
        layoutLoginCard.ResumeLayout(false);
        layoutLoginCard.PerformLayout();
        panelPasswordRow.ResumeLayout(false);
        panelPasswordRow.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private TableLayoutPanel tableLayoutRoot;
    private Panel panelBrand;
    private Label lblBrandFooter;
    private Label lblBrandBody;
    private Label lblBrandTitle;
    private PictureBox pictureBoxLogo;
    private Panel panelLoginCard;
    private TableLayoutPanel layoutLoginCard;
    private Label lblCardEyebrow;
    private Label lblCardTitle;
    private Label lblCardBody;
    private Label lblUsername;
    private TextBox txtUsername;
    private Label lblPassword;
    private Panel panelPasswordRow;
    private Button btnTogglePassword;
    private TextBox txtPassword;
    private Label lblStatus;
    private UgnayDesktop.Controls.RoundedButton btnLogin;
    private Button btnCreateAccount;
}


