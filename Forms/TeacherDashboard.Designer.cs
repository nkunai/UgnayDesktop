namespace UgnayDesktop.Forms
{
    partial class TeacherDashboard
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            panelHeader = new Panel();
            btnLogout = new Button();
            btnSaveProfile = new Button();
            lblTeacherPhone = new Label();
            lblTeacherPhoneCaption = new Label();
            lblTeacherNameValue = new Label();
            lblTeacherNameCaption = new Label();
            lblProfileHeader = new Label();
            panelStats = new TableLayoutPanel();
            pnlTotalStudents = new Panel();
            lblTotalStudentsValue = new Label();
            lblTotalStudentsTitle = new Label();
            pnlConnectedStudents = new Panel();
            lblConnectedStudentsValue = new Label();
            lblConnectedStudentsTitle = new Label();
            pnlDisconnectedStudents = new Panel();
            lblDisconnectedStudentsValue = new Label();
            lblDisconnectedStudentsTitle = new Label();
            pnlAttentionStudents = new Panel();
            lblAttentionStudentsValue = new Label();
            lblAttentionStudentsTitle = new Label();
            lblStudentHeader = new Label();
            txtStudentSearch = new TextBox();
            cmbStudentFilter = new ComboBox();
            cmbStudentSort = new ComboBox();
            btnAddStudent = new Button();
            flpStudentCards = new FlowLayoutPanel();
            panelHeader.SuspendLayout();
            panelStats.SuspendLayout();
            pnlTotalStudents.SuspendLayout();
            pnlConnectedStudents.SuspendLayout();
            pnlDisconnectedStudents.SuspendLayout();
            pnlAttentionStudents.SuspendLayout();
            SuspendLayout();
            // panelHeader
            panelHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelHeader.BackColor = Color.White;
            panelHeader.BorderStyle = BorderStyle.FixedSingle;
            panelHeader.Controls.Add(btnLogout);
            panelHeader.Controls.Add(btnSaveProfile);
            panelHeader.Controls.Add(lblTeacherPhone);
            panelHeader.Controls.Add(lblTeacherPhoneCaption);
            panelHeader.Controls.Add(lblTeacherNameValue);
            panelHeader.Controls.Add(lblTeacherNameCaption);
            panelHeader.Controls.Add(lblProfileHeader);
            panelHeader.Location = new Point(12, 12);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(1260, 118);
            panelHeader.TabIndex = 0;
            // btnLogout
            btnLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLogout.Location = new Point(1128, 64);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(116, 38);
            btnLogout.TabIndex = 6;
            btnLogout.Text = "Logout";
            btnLogout.UseVisualStyleBackColor = true;
            btnLogout.Click += btnLogout_Click;
            // btnSaveProfile
            btnSaveProfile.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSaveProfile.Location = new Point(970, 64);
            btnSaveProfile.Name = "btnSaveProfile";
            btnSaveProfile.Size = new Size(152, 38);
            btnSaveProfile.TabIndex = 5;
            btnSaveProfile.Text = "Edit Profile";
            btnSaveProfile.UseVisualStyleBackColor = true;
            btnSaveProfile.Click += btnSaveProfile_Click;
            // lblTeacherPhone
            lblTeacherPhone.AutoSize = true;
            lblTeacherPhone.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblTeacherPhone.ForeColor = Color.FromArgb(65, 74, 86);
            lblTeacherPhone.Location = new Point(150, 72);
            lblTeacherPhone.Name = "lblTeacherPhone";
            lblTeacherPhone.Size = new Size(74, 28);
            lblTeacherPhone.TabIndex = 4;
            lblTeacherPhone.Text = "Not set";
            // lblTeacherPhoneCaption
            lblTeacherPhoneCaption.AutoSize = true;
            lblTeacherPhoneCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTeacherPhoneCaption.ForeColor = Color.FromArgb(90, 101, 115);
            lblTeacherPhoneCaption.Location = new Point(20, 74);
            lblTeacherPhoneCaption.Name = "lblTeacherPhoneCaption";
            lblTeacherPhoneCaption.Size = new Size(124, 25);
            lblTeacherPhoneCaption.TabIndex = 3;
            lblTeacherPhoneCaption.Text = "Teacher phone";
            // lblTeacherNameValue
            lblTeacherNameValue.AutoSize = true;
            lblTeacherNameValue.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTeacherNameValue.ForeColor = Color.FromArgb(24, 33, 45);
            lblTeacherNameValue.Location = new Point(20, 25);
            lblTeacherNameValue.Name = "lblTeacherNameValue";
            lblTeacherNameValue.Size = new Size(162, 45);
            lblTeacherNameValue.TabIndex = 2;
            lblTeacherNameValue.Text = "Teacher";
            // lblTeacherNameCaption
            lblTeacherNameCaption.AutoSize = true;
            lblTeacherNameCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTeacherNameCaption.ForeColor = Color.FromArgb(90, 101, 115);
            lblTeacherNameCaption.Location = new Point(20, 4);
            lblTeacherNameCaption.Name = "lblTeacherNameCaption";
            lblTeacherNameCaption.Size = new Size(117, 25);
            lblTeacherNameCaption.TabIndex = 1;
            lblTeacherNameCaption.Text = "Teacher name";
            // lblProfileHeader
            lblProfileHeader.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblProfileHeader.AutoSize = true;
            lblProfileHeader.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblProfileHeader.ForeColor = Color.FromArgb(90, 101, 115);
            lblProfileHeader.Location = new Point(968, 23);
            lblProfileHeader.Name = "lblProfileHeader";
            lblProfileHeader.Size = new Size(97, 25);
            lblProfileHeader.TabIndex = 0;
            lblProfileHeader.Text = "My Profile";
            // panelStats
            panelStats.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelStats.ColumnCount = 4;
            panelStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            panelStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            panelStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            panelStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            panelStats.Controls.Add(pnlTotalStudents, 0, 0);
            panelStats.Controls.Add(pnlConnectedStudents, 1, 0);
            panelStats.Controls.Add(pnlDisconnectedStudents, 2, 0);
            panelStats.Controls.Add(pnlAttentionStudents, 3, 0);
            panelStats.Location = new Point(12, 142);
            panelStats.Name = "panelStats";
            panelStats.RowCount = 1;
            panelStats.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            panelStats.Size = new Size(1260, 108);
            panelStats.TabIndex = 1;
            // pnlTotalStudents
            pnlTotalStudents.BackColor = Color.White;
            pnlTotalStudents.BorderStyle = BorderStyle.FixedSingle;
            pnlTotalStudents.Controls.Add(lblTotalStudentsValue);
            pnlTotalStudents.Controls.Add(lblTotalStudentsTitle);
            pnlTotalStudents.Dock = DockStyle.Fill;
            pnlTotalStudents.Location = new Point(3, 3);
            pnlTotalStudents.Name = "pnlTotalStudents";
            pnlTotalStudents.Size = new Size(309, 102);
            pnlTotalStudents.TabIndex = 0;
            // lblTotalStudentsValue
            lblTotalStudentsValue.AutoSize = true;
            lblTotalStudentsValue.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTotalStudentsValue.ForeColor = Color.FromArgb(24, 33, 45);
            lblTotalStudentsValue.Location = new Point(18, 35);
            lblTotalStudentsValue.Name = "lblTotalStudentsValue";
            lblTotalStudentsValue.Size = new Size(46, 54);
            lblTotalStudentsValue.TabIndex = 1;
            lblTotalStudentsValue.Text = "0";
            // lblTotalStudentsTitle
            lblTotalStudentsTitle.AutoSize = true;
            lblTotalStudentsTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTotalStudentsTitle.ForeColor = Color.FromArgb(90, 101, 115);
            lblTotalStudentsTitle.Location = new Point(18, 12);
            lblTotalStudentsTitle.Name = "lblTotalStudentsTitle";
            lblTotalStudentsTitle.Size = new Size(115, 25);
            lblTotalStudentsTitle.TabIndex = 0;
            lblTotalStudentsTitle.Text = "Total students";
            // pnlConnectedStudents
            pnlConnectedStudents.BackColor = Color.White;
            pnlConnectedStudents.BorderStyle = BorderStyle.FixedSingle;
            pnlConnectedStudents.Controls.Add(lblConnectedStudentsValue);
            pnlConnectedStudents.Controls.Add(lblConnectedStudentsTitle);
            pnlConnectedStudents.Dock = DockStyle.Fill;
            pnlConnectedStudents.Location = new Point(318, 3);
            pnlConnectedStudents.Name = "pnlConnectedStudents";
            pnlConnectedStudents.Size = new Size(309, 102);
            pnlConnectedStudents.TabIndex = 1;
            // lblConnectedStudentsValue
            lblConnectedStudentsValue.AutoSize = true;
            lblConnectedStudentsValue.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblConnectedStudentsValue.ForeColor = Color.FromArgb(19, 109, 62);
            lblConnectedStudentsValue.Location = new Point(18, 35);
            lblConnectedStudentsValue.Name = "lblConnectedStudentsValue";
            lblConnectedStudentsValue.Size = new Size(46, 54);
            lblConnectedStudentsValue.TabIndex = 1;
            lblConnectedStudentsValue.Text = "0";
            // lblConnectedStudentsTitle
            lblConnectedStudentsTitle.AutoSize = true;
            lblConnectedStudentsTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblConnectedStudentsTitle.ForeColor = Color.FromArgb(90, 101, 115);
            lblConnectedStudentsTitle.Location = new Point(18, 12);
            lblConnectedStudentsTitle.Name = "lblConnectedStudentsTitle";
            lblConnectedStudentsTitle.Size = new Size(157, 25);
            lblConnectedStudentsTitle.TabIndex = 0;
            lblConnectedStudentsTitle.Text = "Connected students";
            // pnlDisconnectedStudents
            pnlDisconnectedStudents.BackColor = Color.White;
            pnlDisconnectedStudents.BorderStyle = BorderStyle.FixedSingle;
            pnlDisconnectedStudents.Controls.Add(lblDisconnectedStudentsValue);
            pnlDisconnectedStudents.Controls.Add(lblDisconnectedStudentsTitle);
            pnlDisconnectedStudents.Dock = DockStyle.Fill;
            pnlDisconnectedStudents.Location = new Point(633, 3);
            pnlDisconnectedStudents.Name = "pnlDisconnectedStudents";
            pnlDisconnectedStudents.Size = new Size(309, 102);
            pnlDisconnectedStudents.TabIndex = 2;
            // lblDisconnectedStudentsValue
            lblDisconnectedStudentsValue.AutoSize = true;
            lblDisconnectedStudentsValue.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblDisconnectedStudentsValue.ForeColor = Color.FromArgb(163, 64, 50);
            lblDisconnectedStudentsValue.Location = new Point(18, 35);
            lblDisconnectedStudentsValue.Name = "lblDisconnectedStudentsValue";
            lblDisconnectedStudentsValue.Size = new Size(46, 54);
            lblDisconnectedStudentsValue.TabIndex = 1;
            lblDisconnectedStudentsValue.Text = "0";
            // lblDisconnectedStudentsTitle
            lblDisconnectedStudentsTitle.AutoSize = true;
            lblDisconnectedStudentsTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblDisconnectedStudentsTitle.ForeColor = Color.FromArgb(90, 101, 115);
            lblDisconnectedStudentsTitle.Location = new Point(18, 12);
            lblDisconnectedStudentsTitle.Name = "lblDisconnectedStudentsTitle";
            lblDisconnectedStudentsTitle.Size = new Size(182, 25);
            lblDisconnectedStudentsTitle.TabIndex = 0;
            lblDisconnectedStudentsTitle.Text = "Disconnected students";
            // pnlAttentionStudents
            pnlAttentionStudents.BackColor = Color.White;
            pnlAttentionStudents.BorderStyle = BorderStyle.FixedSingle;
            pnlAttentionStudents.Controls.Add(lblAttentionStudentsValue);
            pnlAttentionStudents.Controls.Add(lblAttentionStudentsTitle);
            pnlAttentionStudents.Dock = DockStyle.Fill;
            pnlAttentionStudents.Location = new Point(948, 3);
            pnlAttentionStudents.Name = "pnlAttentionStudents";
            pnlAttentionStudents.Size = new Size(309, 102);
            pnlAttentionStudents.TabIndex = 3;
            // lblAttentionStudentsValue
            lblAttentionStudentsValue.AutoSize = true;
            lblAttentionStudentsValue.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblAttentionStudentsValue.ForeColor = Color.FromArgb(178, 106, 12);
            lblAttentionStudentsValue.Location = new Point(18, 35);
            lblAttentionStudentsValue.Name = "lblAttentionStudentsValue";
            lblAttentionStudentsValue.Size = new Size(46, 54);
            lblAttentionStudentsValue.TabIndex = 1;
            lblAttentionStudentsValue.Text = "0";
            // lblAttentionStudentsTitle
            lblAttentionStudentsTitle.AutoSize = true;
            lblAttentionStudentsTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblAttentionStudentsTitle.ForeColor = Color.FromArgb(90, 101, 115);
            lblAttentionStudentsTitle.Location = new Point(18, 12);
            lblAttentionStudentsTitle.Name = "lblAttentionStudentsTitle";
            lblAttentionStudentsTitle.Size = new Size(130, 25);
            lblAttentionStudentsTitle.TabIndex = 0;
            lblAttentionStudentsTitle.Text = "Needs attention";
            // lblStudentHeader
            lblStudentHeader.AutoSize = true;
            lblStudentHeader.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblStudentHeader.Location = new Point(12, 267);
            lblStudentHeader.Name = "lblStudentHeader";
            lblStudentHeader.Size = new Size(165, 32);
            lblStudentHeader.TabIndex = 2;
            lblStudentHeader.Text = "Student cards";
            // txtStudentSearch
            txtStudentSearch.Location = new Point(12, 311);
            txtStudentSearch.Name = "txtStudentSearch";
            txtStudentSearch.PlaceholderText = "Search by student name or device ID";
            txtStudentSearch.Size = new Size(360, 31);
            txtStudentSearch.TabIndex = 3;
            txtStudentSearch.TextChanged += txtStudentSearch_TextChanged;
            // cmbStudentFilter
            cmbStudentFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStudentFilter.FormattingEnabled = true;
            cmbStudentFilter.Location = new Point(378, 311);
            cmbStudentFilter.Name = "cmbStudentFilter";
            cmbStudentFilter.Size = new Size(200, 33);
            cmbStudentFilter.TabIndex = 4;
            cmbStudentFilter.SelectedIndexChanged += cmbStudentFilter_SelectedIndexChanged;
            // cmbStudentSort
            cmbStudentSort.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStudentSort.FormattingEnabled = true;
            cmbStudentSort.Location = new Point(584, 311);
            cmbStudentSort.Name = "cmbStudentSort";
            cmbStudentSort.Size = new Size(220, 33);
            cmbStudentSort.TabIndex = 5;
            cmbStudentSort.SelectedIndexChanged += cmbStudentSort_SelectedIndexChanged;
            // btnAddStudent
            btnAddStudent.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAddStudent.Location = new Point(1128, 307);
            btnAddStudent.Name = "btnAddStudent";
            btnAddStudent.Size = new Size(144, 40);
            btnAddStudent.TabIndex = 6;
            btnAddStudent.Text = "Add Student";
            btnAddStudent.UseVisualStyleBackColor = true;
            btnAddStudent.Click += btnAddStudent_Click;
            // flpStudentCards
            flpStudentCards.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            flpStudentCards.AutoScroll = true;
            flpStudentCards.BackColor = Color.FromArgb(245, 247, 250);
            flpStudentCards.FlowDirection = FlowDirection.LeftToRight;
            flpStudentCards.Location = new Point(12, 360);
            flpStudentCards.Name = "flpStudentCards";
            flpStudentCards.Padding = new Padding(12);
            flpStudentCards.Size = new Size(1260, 488);
            flpStudentCards.TabIndex = 7;
            flpStudentCards.WrapContents = true;
            // TeacherDashboard
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 247, 250);
            ClientSize = new Size(1284, 860);
            Controls.Add(flpStudentCards);
            Controls.Add(btnAddStudent);
            Controls.Add(cmbStudentSort);
            Controls.Add(cmbStudentFilter);
            Controls.Add(txtStudentSearch);
            Controls.Add(lblStudentHeader);
            Controls.Add(panelStats);
            Controls.Add(panelHeader);
            MinimumSize = new Size(1100, 760);
            Name = "TeacherDashboard";
            Text = "Teacher Dashboard";
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            panelStats.ResumeLayout(false);
            pnlTotalStudents.ResumeLayout(false);
            pnlTotalStudents.PerformLayout();
            pnlConnectedStudents.ResumeLayout(false);
            pnlConnectedStudents.PerformLayout();
            pnlDisconnectedStudents.ResumeLayout(false);
            pnlDisconnectedStudents.PerformLayout();
            pnlAttentionStudents.ResumeLayout(false);
            pnlAttentionStudents.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelHeader;
        private Button btnLogout;
        private Button btnSaveProfile;
        private Label lblTeacherPhone;
        private Label lblTeacherPhoneCaption;
        private Label lblTeacherNameValue;
        private Label lblTeacherNameCaption;
        private Label lblProfileHeader;
        private TableLayoutPanel panelStats;
        private Panel pnlTotalStudents;
        private Label lblTotalStudentsValue;
        private Label lblTotalStudentsTitle;
        private Panel pnlConnectedStudents;
        private Label lblConnectedStudentsValue;
        private Label lblConnectedStudentsTitle;
        private Panel pnlDisconnectedStudents;
        private Label lblDisconnectedStudentsValue;
        private Label lblDisconnectedStudentsTitle;
        private Panel pnlAttentionStudents;
        private Label lblAttentionStudentsValue;
        private Label lblAttentionStudentsTitle;
        private Label lblStudentHeader;
        private TextBox txtStudentSearch;
        private ComboBox cmbStudentFilter;
        private ComboBox cmbStudentSort;
        private Button btnAddStudent;
        private FlowLayoutPanel flpStudentCards;
    }
}
