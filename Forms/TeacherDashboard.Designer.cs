namespace UgnayDesktop.Forms
{
    partial class TeacherDashboard
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dgvSensorReadings = new DataGridView();
            lblDecisionStatus = new Label();
            btnLogout = new Button();
            lblProfileHeader = new Label();
            labelTeacherName = new Label();
            txtTeacherFullName = new TextBox();
            labelTeacherPhoneEdit = new Label();
            lblTeacherPhonePrefix = new Label();
            txtTeacherPhoneSuffix = new TextBox();
            btnSaveProfile = new Button();
            lblTeacherPhone = new Label();
            btnTwilioConfigCheck = new Button();
            btnTwilioLink = new Button();
            btnTwilioTest = new Button();
            grpManualAlerts = new GroupBox();
            lblBpmAlert = new Label();
            txtBpmAlertMessage = new TextBox();
            btnBpmAlert = new Button();
            lblSweatnessAlert = new Label();
            txtSweatnessAlertMessage = new TextBox();
            btnSweatnessAlert = new Button();
            lblTemperatureAlert = new Label();
            txtTemperatureAlertMessage = new TextBox();
            btnTemperatureAlert = new Button();
            lblStudentHeader = new Label();
            labelFullName = new Label();
            txtStudentFullName = new TextBox();
            labelAge = new Label();
            txtStudentAge = new TextBox();
            labelSex = new Label();
            cmbStudentSex = new ComboBox();
            btnAddStudent = new Button();
            dgvStudents = new DataGridView();
            lblConnectionStatus = new Label();
            lblSelectedStudent = new Label();
            flpStudentCards = new FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)dgvSensorReadings).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvStudents).BeginInit();
            grpManualAlerts.SuspendLayout();
            SuspendLayout();
            // 
            // dgvSensorReadings
            // 
            dgvSensorReadings.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvSensorReadings.Location = new Point(12, 561);
            dgvSensorReadings.Name = "dgvSensorReadings";
            dgvSensorReadings.RowHeadersWidth = 72;
            dgvSensorReadings.Size = new Size(1160, 346);
            dgvSensorReadings.TabIndex = 0;
            // 
            // lblDecisionStatus
            // 
            lblDecisionStatus.AutoSize = true;
            lblDecisionStatus.Location = new Point(12, 525);
            lblDecisionStatus.Name = "lblDecisionStatus";
            lblDecisionStatus.Size = new Size(92, 30);
            lblDecisionStatus.TabIndex = 1;
            lblDecisionStatus.Text = "Decision";
            lblDecisionStatus.Visible = false;
            // 
            // btnLogout
            // 
            btnLogout.Location = new Point(1041, 12);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(131, 40);
            btnLogout.TabIndex = 2;
            btnLogout.Text = "Logout";
            btnLogout.UseVisualStyleBackColor = true;
            btnLogout.Click += btnLogout_Click;
            // 
            // lblProfileHeader
            // 
            lblProfileHeader.AutoSize = true;
            lblProfileHeader.Location = new Point(12, 12);
            lblProfileHeader.Name = "lblProfileHeader";
            lblProfileHeader.Size = new Size(105, 30);
            lblProfileHeader.TabIndex = 3;
            lblProfileHeader.Text = "My Profile";
            // 
            // labelTeacherName
            // 
            labelTeacherName.AutoSize = true;
            labelTeacherName.Location = new Point(12, 49);
            labelTeacherName.Name = "labelTeacherName";
            labelTeacherName.Size = new Size(107, 30);
            labelTeacherName.TabIndex = 4;
            labelTeacherName.Text = "Full Name";
            labelTeacherName.Visible = false;
            // 
            // txtTeacherFullName
            // 
            txtTeacherFullName.Location = new Point(12, 82);
            txtTeacherFullName.Name = "txtTeacherFullName";
            txtTeacherFullName.Size = new Size(262, 35);
            txtTeacherFullName.TabIndex = 5;
            txtTeacherFullName.Visible = false;
            // 
            // labelTeacherPhoneEdit
            // 
            labelTeacherPhoneEdit.AutoSize = true;
            labelTeacherPhoneEdit.Location = new Point(280, 49);
            labelTeacherPhoneEdit.Name = "labelTeacherPhoneEdit";
            labelTeacherPhoneEdit.Size = new Size(156, 30);
            labelTeacherPhoneEdit.TabIndex = 6;
            labelTeacherPhoneEdit.Text = "Teacher Phone";
            labelTeacherPhoneEdit.Visible = false;
            // 
            // lblTeacherPhonePrefix
            // 
            lblTeacherPhonePrefix.AutoSize = true;
            lblTeacherPhonePrefix.Location = new Point(280, 85);
            lblTeacherPhonePrefix.Name = "lblTeacherPhonePrefix";
            lblTeacherPhonePrefix.Size = new Size(58, 30);
            lblTeacherPhonePrefix.TabIndex = 7;
            lblTeacherPhonePrefix.Text = "+63";
            lblTeacherPhonePrefix.Visible = false;
            // 
            // txtTeacherPhoneSuffix
            // 
            txtTeacherPhoneSuffix.Location = new Point(342, 82);
            txtTeacherPhoneSuffix.MaxLength = 10;
            txtTeacherPhoneSuffix.Name = "txtTeacherPhoneSuffix";
            txtTeacherPhoneSuffix.Size = new Size(200, 35);
            txtTeacherPhoneSuffix.TabIndex = 8;
            txtTeacherPhoneSuffix.KeyPress += txtTeacherPhoneSuffix_KeyPress;
            txtTeacherPhoneSuffix.Visible = false;
            // 
            // btnSaveProfile
            // 
            btnSaveProfile.Location = new Point(548, 80);
            btnSaveProfile.Name = "btnSaveProfile";
            btnSaveProfile.Size = new Size(156, 40);
            btnSaveProfile.TabIndex = 9;
            btnSaveProfile.Text = "Edit Profile";
            btnSaveProfile.UseVisualStyleBackColor = true;
            btnSaveProfile.Click += btnSaveProfile_Click;
            // 
            // lblTeacherPhone
            // 
            lblTeacherPhone.AutoSize = true;
            lblTeacherPhone.Location = new Point(12, 120);
            lblTeacherPhone.Name = "lblTeacherPhone";
            lblTeacherPhone.Size = new Size(156, 30);
            lblTeacherPhone.TabIndex = 10;
            lblTeacherPhone.Text = "Teacher Phone:";
            // 
            // btnTwilioConfigCheck
            // 
            btnTwilioConfigCheck.Location = new Point(630, 12);
            btnTwilioConfigCheck.Name = "btnTwilioConfigCheck";
            btnTwilioConfigCheck.Size = new Size(131, 40);
            btnTwilioConfigCheck.TabIndex = 11;
            btnTwilioConfigCheck.Text = "Config Check";
            btnTwilioConfigCheck.UseVisualStyleBackColor = true;
            btnTwilioConfigCheck.Click += btnTwilioConfigCheck_Click;
            // 
            // btnTwilioLink
            // 
            btnTwilioLink.Location = new Point(767, 12);
            btnTwilioLink.Name = "btnTwilioLink";
            btnTwilioLink.Size = new Size(131, 40);
            btnTwilioLink.TabIndex = 12;
            btnTwilioLink.Text = "TextBee Link";
            btnTwilioLink.UseVisualStyleBackColor = true;
            btnTwilioLink.Click += btnTwilioLink_Click;
            // 
            // btnTwilioTest
            // 
            btnTwilioTest.Location = new Point(904, 12);
            btnTwilioTest.Name = "btnTwilioTest";
            btnTwilioTest.Size = new Size(131, 40);
            btnTwilioTest.TabIndex = 13;
            btnTwilioTest.Text = "TextBee Test";
            btnTwilioTest.UseVisualStyleBackColor = true;
            btnTwilioTest.Click += btnTwilioTest_Click;
            // 
            // grpManualAlerts
            // 
            grpManualAlerts.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            grpManualAlerts.Controls.Add(btnTemperatureAlert);
            grpManualAlerts.Controls.Add(txtTemperatureAlertMessage);
            grpManualAlerts.Controls.Add(lblTemperatureAlert);
            grpManualAlerts.Controls.Add(btnSweatnessAlert);
            grpManualAlerts.Controls.Add(txtSweatnessAlertMessage);
            grpManualAlerts.Controls.Add(lblSweatnessAlert);
            grpManualAlerts.Controls.Add(btnBpmAlert);
            grpManualAlerts.Controls.Add(txtBpmAlertMessage);
            grpManualAlerts.Controls.Add(lblBpmAlert);
            grpManualAlerts.Location = new Point(710, 58);
            grpManualAlerts.Name = "grpManualAlerts";
            grpManualAlerts.Size = new Size(462, 142);
            grpManualAlerts.TabIndex = 14;
            grpManualAlerts.TabStop = false;
            grpManualAlerts.Text = "Manual Alerts";
            // 
            // lblBpmAlert
            // 
            lblBpmAlert.AutoSize = true;
            lblBpmAlert.Location = new Point(12, 32);
            lblBpmAlert.Name = "lblBpmAlert";
            lblBpmAlert.Size = new Size(58, 30);
            lblBpmAlert.TabIndex = 0;
            lblBpmAlert.Text = "BPM";
            // 
            // txtBpmAlertMessage
            // 
            txtBpmAlertMessage.Location = new Point(97, 29);
            txtBpmAlertMessage.Name = "txtBpmAlertMessage";
            txtBpmAlertMessage.Size = new Size(233, 35);
            txtBpmAlertMessage.TabIndex = 1;
            txtBpmAlertMessage.Text = "BPM is too high";
            // 
            // btnBpmAlert
            // 
            btnBpmAlert.Location = new Point(336, 28);
            btnBpmAlert.Name = "btnBpmAlert";
            btnBpmAlert.Size = new Size(112, 37);
            btnBpmAlert.TabIndex = 2;
            btnBpmAlert.Text = "BPM Alert";
            btnBpmAlert.UseVisualStyleBackColor = true;
            btnBpmAlert.Click += btnBpmAlert_Click;
            // 
            // lblSweatnessAlert
            // 
            lblSweatnessAlert.AutoSize = true;
            lblSweatnessAlert.Location = new Point(12, 67);
            lblSweatnessAlert.Name = "lblSweatnessAlert";
            lblSweatnessAlert.Size = new Size(124, 30);
            lblSweatnessAlert.TabIndex = 3;
            lblSweatnessAlert.Text = "Sweatness";
            // 
            // txtSweatnessAlertMessage
            // 
            txtSweatnessAlertMessage.Location = new Point(142, 64);
            txtSweatnessAlertMessage.Name = "txtSweatnessAlertMessage";
            txtSweatnessAlertMessage.Size = new Size(188, 35);
            txtSweatnessAlertMessage.TabIndex = 4;
            txtSweatnessAlertMessage.Text = "sweatness level is too high";
            // 
            // btnSweatnessAlert
            // 
            btnSweatnessAlert.Location = new Point(336, 63);
            btnSweatnessAlert.Name = "btnSweatnessAlert";
            btnSweatnessAlert.Size = new Size(112, 37);
            btnSweatnessAlert.TabIndex = 5;
            btnSweatnessAlert.Text = "Sweatness Alert";
            btnSweatnessAlert.UseVisualStyleBackColor = true;
            btnSweatnessAlert.Click += btnSweatnessAlert_Click;
            // 
            // lblTemperatureAlert
            // 
            lblTemperatureAlert.AutoSize = true;
            lblTemperatureAlert.Location = new Point(12, 102);
            lblTemperatureAlert.Name = "lblTemperatureAlert";
            lblTemperatureAlert.Size = new Size(140, 30);
            lblTemperatureAlert.TabIndex = 6;
            lblTemperatureAlert.Text = "Temperature";
            // 
            // txtTemperatureAlertMessage
            // 
            txtTemperatureAlertMessage.Location = new Point(158, 99);
            txtTemperatureAlertMessage.Name = "txtTemperatureAlertMessage";
            txtTemperatureAlertMessage.Size = new Size(172, 35);
            txtTemperatureAlertMessage.TabIndex = 7;
            txtTemperatureAlertMessage.Text = "Temperature is too high";
            // 
            // btnTemperatureAlert
            // 
            btnTemperatureAlert.Location = new Point(336, 98);
            btnTemperatureAlert.Name = "btnTemperatureAlert";
            btnTemperatureAlert.Size = new Size(112, 37);
            btnTemperatureAlert.TabIndex = 8;
            btnTemperatureAlert.Text = "Temperature Alert";
            btnTemperatureAlert.UseVisualStyleBackColor = true;
            btnTemperatureAlert.Click += btnTemperatureAlert_Click;
            // lblStudentHeader
            // 
            lblStudentHeader.AutoSize = true;
            lblStudentHeader.Location = new Point(12, 164);
            lblStudentHeader.Name = "lblStudentHeader";
            lblStudentHeader.Size = new Size(243, 30);
            lblStudentHeader.TabIndex = 14;
            lblStudentHeader.Text = "Student Cards";
            // 
            // labelFullName
            // 
            labelFullName.AutoSize = true;
            labelFullName.Location = new Point(12, 204);
            labelFullName.Name = "labelFullName";
            labelFullName.Size = new Size(107, 30);
            labelFullName.TabIndex = 15;
            labelFullName.Text = "Full Name";
            labelFullName.Visible = false;
            // 
            // txtStudentFullName
            // 
            txtStudentFullName.Location = new Point(12, 237);
            txtStudentFullName.Name = "txtStudentFullName";
            txtStudentFullName.Size = new Size(324, 35);
            txtStudentFullName.TabIndex = 16;
            txtStudentFullName.Visible = false;
            // 
            // labelAge
            // 
            labelAge.AutoSize = true;
            labelAge.Location = new Point(342, 204);
            labelAge.Name = "labelAge";
            labelAge.Size = new Size(50, 30);
            labelAge.TabIndex = 17;
            labelAge.Text = "Age";
            labelAge.Visible = false;
            // 
            // txtStudentAge
            // 
            txtStudentAge.Location = new Point(342, 237);
            txtStudentAge.Name = "txtStudentAge";
            txtStudentAge.Size = new Size(110, 35);
            txtStudentAge.TabIndex = 18;
            txtStudentAge.Visible = false;
            // 
            // labelSex
            // 
            labelSex.AutoSize = true;
            labelSex.Location = new Point(458, 204);
            labelSex.Name = "labelSex";
            labelSex.Size = new Size(45, 30);
            labelSex.TabIndex = 19;
            labelSex.Text = "Sex";
            labelSex.Visible = false;
            // 
            // cmbStudentSex
            // 
            cmbStudentSex.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStudentSex.FormattingEnabled = true;
            cmbStudentSex.Items.AddRange(new object[] { "Male", "Female" });
            cmbStudentSex.Location = new Point(458, 236);
            cmbStudentSex.Name = "cmbStudentSex";
            cmbStudentSex.Size = new Size(143, 38);
            cmbStudentSex.TabIndex = 20;
            cmbStudentSex.Visible = false;
            // 
            // btnAddStudent
            // 
            btnAddStudent.Location = new Point(170, 161);
            btnAddStudent.Name = "btnAddStudent";
            btnAddStudent.Size = new Size(154, 40);
            btnAddStudent.TabIndex = 21;
            btnAddStudent.Text = "Add Student";
            btnAddStudent.UseVisualStyleBackColor = true;
            btnAddStudent.Click += btnAddStudent_Click;
            // 
            // dgvStudents
            // 
            dgvStudents.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvStudents.Location = new Point(12, 288);
            dgvStudents.Name = "dgvStudents";
            dgvStudents.RowHeadersWidth = 72;
            dgvStudents.Size = new Size(1160, 189);
            dgvStudents.TabIndex = 22;
            dgvStudents.Visible = false;
            // 
            // lblConnectionStatus
            // 
            lblConnectionStatus.AutoSize = true;
            lblConnectionStatus.Location = new Point(12, 495);
            lblConnectionStatus.Name = "lblConnectionStatus";
            lblConnectionStatus.Size = new Size(124, 30);
            lblConnectionStatus.TabIndex = 23;
            lblConnectionStatus.Text = "Connection:";
            lblConnectionStatus.Visible = false;
            // 
            // lblSelectedStudent
            // 
            lblSelectedStudent.AutoSize = true;
            lblSelectedStudent.Location = new Point(12, 465);
            lblSelectedStudent.Name = "lblSelectedStudent";
            lblSelectedStudent.Size = new Size(173, 30);
            lblSelectedStudent.TabIndex = 24;
            lblSelectedStudent.Text = "Selected Student:";
            lblSelectedStudent.Visible = false;
            // 
            // flpStudentCards
            // 
            flpStudentCards.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            flpStudentCards.AutoScroll = true;
            flpStudentCards.BackColor = Color.FromArgb(245, 247, 250);
            flpStudentCards.FlowDirection = FlowDirection.LeftToRight;
            flpStudentCards.Location = new Point(12, 206);
            flpStudentCards.Name = "flpStudentCards";
            flpStudentCards.Padding = new Padding(10);
            flpStudentCards.Size = new Size(1160, 702);
            flpStudentCards.TabIndex = 25;
            flpStudentCards.WrapContents = true;
            // 
            // TeacherDashboard
            // 
            AutoScaleDimensions = new SizeF(12F, 30F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1184, 920);
            Controls.Add(lblSelectedStudent);
            Controls.Add(lblConnectionStatus);
            Controls.Add(dgvStudents);
            Controls.Add(btnAddStudent);
            Controls.Add(flpStudentCards);
            Controls.Add(grpManualAlerts);
            Controls.Add(cmbStudentSex);
            Controls.Add(labelSex);
            Controls.Add(txtStudentAge);
            Controls.Add(labelAge);
            Controls.Add(txtStudentFullName);
            Controls.Add(labelFullName);
            Controls.Add(lblStudentHeader);
            Controls.Add(btnTwilioTest);
            Controls.Add(btnTwilioLink);
            Controls.Add(btnTwilioConfigCheck);
            Controls.Add(lblTeacherPhone);
            Controls.Add(btnSaveProfile);
            Controls.Add(txtTeacherPhoneSuffix);
            Controls.Add(lblTeacherPhonePrefix);
            Controls.Add(labelTeacherPhoneEdit);
            Controls.Add(txtTeacherFullName);
            Controls.Add(labelTeacherName);
            Controls.Add(lblProfileHeader);
            Controls.Add(btnLogout);
            Controls.Add(lblDecisionStatus);
            Controls.Add(dgvSensorReadings);
            dgvSensorReadings.Visible = false;
            Name = "TeacherDashboard";
            Text = "Teacher Dashboard";
            ((System.ComponentModel.ISupportInitialize)dgvSensorReadings).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvStudents).EndInit();
            grpManualAlerts.ResumeLayout(false);
            grpManualAlerts.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgvSensorReadings;
        private Label lblDecisionStatus;
        private Button btnLogout;
        private Label lblProfileHeader;
        private Label labelTeacherName;
        private TextBox txtTeacherFullName;
        private Label labelTeacherPhoneEdit;
        private Label lblTeacherPhonePrefix;
        private TextBox txtTeacherPhoneSuffix;
        private Button btnSaveProfile;
        private Label lblTeacherPhone;
        private Button btnTwilioConfigCheck;
        private Button btnTwilioLink;
        private Button btnTwilioTest;
        private GroupBox grpManualAlerts;
        private Label lblBpmAlert;
        private TextBox txtBpmAlertMessage;
        private Button btnBpmAlert;
        private Label lblSweatnessAlert;
        private TextBox txtSweatnessAlertMessage;
        private Button btnSweatnessAlert;
        private Label lblTemperatureAlert;
        private TextBox txtTemperatureAlertMessage;
        private Button btnTemperatureAlert;
        private Label lblStudentHeader;
        private Label labelFullName;
        private TextBox txtStudentFullName;
        private Label labelAge;
        private TextBox txtStudentAge;
        private Label labelSex;
        private ComboBox cmbStudentSex;
        private Button btnAddStudent;
        private DataGridView dgvStudents;
        private Label lblConnectionStatus;
        private Label lblSelectedStudent;
        private FlowLayoutPanel flpStudentCards;
    }
}







