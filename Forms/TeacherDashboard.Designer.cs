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
            lblCameraPreviewTitle = new Label();
            picCameraPreview = new PictureBox();
            lblCameraPreviewStatus = new Label();
            chkDarkTheme = new CheckBox();
            lblKpiStudentReadings = new Label();
            lblKpiLatestGesture = new Label();
            lblKpiGestureQuality = new Label();
            lblKpiStudentAlerts = new Label();
            pnlStudentVitalsTrend = new Panel();
            pnlStudentConfidenceTrend = new Panel();
            lblStudentHeader = new Label();
            labelFullName = new Label();
            txtStudentFullName = new TextBox();
            labelAge = new Label();
            txtStudentAge = new TextBox();
            labelSex = new Label();
            cmbStudentSex = new ComboBox();
            btnAddStudent = new Button();
            txtStudentSensorSearch = new TextBox();
            cmbStudentGestureFilter = new ComboBox();
            cmbStudentSensorWindow = new ComboBox();
            chkStudentSensorAlertOnly = new CheckBox();
            btnStudentFilterClear = new Button();
            dgvStudents = new DataGridView();
            lblSelectedStudent = new Label();
            lblConnectionStatus = new Label();
            lblAlertHistoryTitle = new Label();
            cmbAlertHistoryStatus = new ComboBox();
            btnAlertHistoryRefresh = new Button();
            lblAlertHistoryCount = new Label();
            dgvAlertHistory = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)dgvSensorReadings).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picCameraPreview).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvStudents).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvAlertHistory).BeginInit();
            SuspendLayout();
            // 
            // dgvSensorReadings
            // 
            dgvSensorReadings.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvSensorReadings.Location = new Point(12, 801);
            dgvSensorReadings.Name = "dgvSensorReadings";
            dgvSensorReadings.RowHeadersWidth = 72;
            dgvSensorReadings.Size = new Size(1001, 346);
            dgvSensorReadings.TabIndex = 0;
            // 
            // lblDecisionStatus
            // 
            lblDecisionStatus.Location = new Point(12, 1181);
            lblDecisionStatus.Name = "lblDecisionStatus";
            lblDecisionStatus.Size = new Size(760, 55);
            lblDecisionStatus.TabIndex = 1;
            lblDecisionStatus.Text = "Decision";
            // 
            // btnLogout
            // 
            btnLogout.Location = new Point(1364, 10);
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
            lblProfileHeader.Size = new Size(107, 30);
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
            // 
            // txtTeacherFullName
            // 
            txtTeacherFullName.Location = new Point(12, 82);
            txtTeacherFullName.Name = "txtTeacherFullName";
            txtTeacherFullName.Size = new Size(262, 35);
            txtTeacherFullName.TabIndex = 5;
            // 
            // labelTeacherPhoneEdit
            // 
            labelTeacherPhoneEdit.AutoSize = true;
            labelTeacherPhoneEdit.Location = new Point(280, 49);
            labelTeacherPhoneEdit.Name = "labelTeacherPhoneEdit";
            labelTeacherPhoneEdit.Size = new Size(149, 30);
            labelTeacherPhoneEdit.TabIndex = 6;
            labelTeacherPhoneEdit.Text = "Teacher Phone";
            // 
            // lblTeacherPhonePrefix
            // 
            lblTeacherPhonePrefix.AutoSize = true;
            lblTeacherPhonePrefix.Location = new Point(280, 85);
            lblTeacherPhonePrefix.Name = "lblTeacherPhonePrefix";
            lblTeacherPhonePrefix.Size = new Size(60, 30);
            lblTeacherPhonePrefix.TabIndex = 7;
            lblTeacherPhonePrefix.Text = "+639";
            // 
            // txtTeacherPhoneSuffix
            // 
            txtTeacherPhoneSuffix.Location = new Point(342, 82);
            txtTeacherPhoneSuffix.MaxLength = 9;
            txtTeacherPhoneSuffix.Name = "txtTeacherPhoneSuffix";
            txtTeacherPhoneSuffix.Size = new Size(200, 35);
            txtTeacherPhoneSuffix.TabIndex = 8;
            txtTeacherPhoneSuffix.KeyPress += txtTeacherPhoneSuffix_KeyPress;
            // 
            // btnSaveProfile
            // 
            btnSaveProfile.Location = new Point(548, 80);
            btnSaveProfile.Name = "btnSaveProfile";
            btnSaveProfile.Size = new Size(156, 40);
            btnSaveProfile.TabIndex = 9;
            btnSaveProfile.Text = "Save Profile";
            btnSaveProfile.UseVisualStyleBackColor = true;
            btnSaveProfile.Click += btnSaveProfile_Click;
            // 
            // lblTeacherPhone
            // 
            lblTeacherPhone.AutoSize = true;
            lblTeacherPhone.Location = new Point(12, 120);
            lblTeacherPhone.Name = "lblTeacherPhone";
            lblTeacherPhone.Size = new Size(154, 30);
            lblTeacherPhone.TabIndex = 10;
            lblTeacherPhone.Text = "Teacher Phone:";
            // 
            // btnTwilioConfigCheck
            // 
            btnTwilioConfigCheck.Location = new Point(387, 12);
            btnTwilioConfigCheck.Name = "btnTwilioConfigCheck";
            btnTwilioConfigCheck.Size = new Size(131, 40);
            btnTwilioConfigCheck.TabIndex = 11;
            btnTwilioConfigCheck.Text = "Config Check";
            btnTwilioConfigCheck.UseVisualStyleBackColor = true;
            btnTwilioConfigCheck.Click += btnTwilioConfigCheck_Click;
            // 
            // btnTwilioLink
            // 
            btnTwilioLink.Location = new Point(524, 12);
            btnTwilioLink.Name = "btnTwilioLink";
            btnTwilioLink.Size = new Size(131, 40);
            btnTwilioLink.TabIndex = 12;
            btnTwilioLink.Text = "Twilio Link";
            btnTwilioLink.UseVisualStyleBackColor = true;
            btnTwilioLink.Click += btnTwilioLink_Click;
            // 
            // btnTwilioTest
            // 
            btnTwilioTest.Location = new Point(661, 12);
            btnTwilioTest.Name = "btnTwilioTest";
            btnTwilioTest.Size = new Size(131, 40);
            btnTwilioTest.TabIndex = 13;
            btnTwilioTest.Text = "Twilio Test";
            btnTwilioTest.UseVisualStyleBackColor = true;
            btnTwilioTest.Click += btnTwilioTest_Click;
            // 
            // lblCameraPreviewTitle
            // 
            lblCameraPreviewTitle.AutoSize = true;
            lblCameraPreviewTitle.Location = new Point(516, 292);
            lblCameraPreviewTitle.Name = "lblCameraPreviewTitle";
            lblCameraPreviewTitle.Size = new Size(161, 30);
            lblCameraPreviewTitle.TabIndex = 14;
            lblCameraPreviewTitle.Text = "Camera Preview";
            // 
            // picCameraPreview
            // 
            picCameraPreview.BackColor = SystemColors.ControlLight;
            picCameraPreview.BorderStyle = BorderStyle.FixedSingle;
            picCameraPreview.Location = new Point(516, 325);
            picCameraPreview.Name = "picCameraPreview";
            picCameraPreview.Size = new Size(497, 272);
            picCameraPreview.SizeMode = PictureBoxSizeMode.Zoom;
            picCameraPreview.TabIndex = 15;
            picCameraPreview.TabStop = false;
            // 
            // lblCameraPreviewStatus
            // 
            lblCameraPreviewStatus.Location = new Point(516, 600);
            lblCameraPreviewStatus.Name = "lblCameraPreviewStatus";
            lblCameraPreviewStatus.Size = new Size(476, 52);
            lblCameraPreviewStatus.TabIndex = 16;
            lblCameraPreviewStatus.Text = "Camera preview: select a student";
            // 
            // chkDarkTheme
            // 
            chkDarkTheme.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            chkDarkTheme.AutoSize = true;
            chkDarkTheme.Location = new Point(964, 16);
            chkDarkTheme.Name = "chkDarkTheme";
            chkDarkTheme.Size = new Size(147, 34);
            chkDarkTheme.TabIndex = 17;
            chkDarkTheme.Text = "Dark theme";
            chkDarkTheme.UseVisualStyleBackColor = true;
            // 
            // lblKpiStudentReadings
            // 
            lblKpiStudentReadings.BorderStyle = BorderStyle.FixedSingle;
            lblKpiStudentReadings.Location = new Point(1086, 325);
            lblKpiStudentReadings.Name = "lblKpiStudentReadings";
            lblKpiStudentReadings.Padding = new Padding(8, 0, 8, 0);
            lblKpiStudentReadings.Size = new Size(476, 34);
            lblKpiStudentReadings.TabIndex = 18;
            lblKpiStudentReadings.Text = "Connected students: -";
            lblKpiStudentReadings.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblKpiLatestGesture
            // 
            lblKpiLatestGesture.BorderStyle = BorderStyle.FixedSingle;
            lblKpiLatestGesture.Location = new Point(1086, 414);
            lblKpiLatestGesture.Name = "lblKpiLatestGesture";
            lblKpiLatestGesture.Padding = new Padding(8, 0, 8, 0);
            lblKpiLatestGesture.Size = new Size(235, 34);
            lblKpiLatestGesture.TabIndex = 19;
            lblKpiLatestGesture.Text = "Last gesture: -";
            lblKpiLatestGesture.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblKpiGestureQuality
            // 
            lblKpiGestureQuality.BorderStyle = BorderStyle.FixedSingle;
            lblKpiGestureQuality.Location = new Point(1086, 372);
            lblKpiGestureQuality.Name = "lblKpiGestureQuality";
            lblKpiGestureQuality.Padding = new Padding(8, 0, 8, 0);
            lblKpiGestureQuality.Size = new Size(476, 34);
            lblKpiGestureQuality.TabIndex = 20;
            lblKpiGestureQuality.Text = "Avg confidence: -";
            lblKpiGestureQuality.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblKpiStudentAlerts
            // 
            lblKpiStudentAlerts.BorderStyle = BorderStyle.FixedSingle;
            lblKpiStudentAlerts.Location = new Point(1327, 414);
            lblKpiStudentAlerts.Name = "lblKpiStudentAlerts";
            lblKpiStudentAlerts.Padding = new Padding(8, 0, 8, 0);
            lblKpiStudentAlerts.Size = new Size(235, 34);
            lblKpiStudentAlerts.TabIndex = 21;
            lblKpiStudentAlerts.Text = "Active alerts: -";
            lblKpiStudentAlerts.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlStudentVitalsTrend
            // 
            pnlStudentVitalsTrend.BorderStyle = BorderStyle.FixedSingle;
            pnlStudentVitalsTrend.Location = new Point(1086, 457);
            pnlStudentVitalsTrend.Name = "pnlStudentVitalsTrend";
            pnlStudentVitalsTrend.Size = new Size(476, 28);
            pnlStudentVitalsTrend.TabIndex = 22;
            // 
            // pnlStudentConfidenceTrend
            // 
            pnlStudentConfidenceTrend.BorderStyle = BorderStyle.FixedSingle;
            pnlStudentConfidenceTrend.Location = new Point(1086, 491);
            pnlStudentConfidenceTrend.Name = "pnlStudentConfidenceTrend";
            pnlStudentConfidenceTrend.Size = new Size(476, 28);
            pnlStudentConfidenceTrend.TabIndex = 23;
            // 
            // lblStudentHeader
            // 
            lblStudentHeader.AutoSize = true;
            lblStudentHeader.Location = new Point(12, 164);
            lblStudentHeader.Name = "lblStudentHeader";
            lblStudentHeader.Size = new Size(243, 30);
            lblStudentHeader.TabIndex = 21;
            lblStudentHeader.Text = "Add Student Information";
            // 
            // labelFullName
            // 
            labelFullName.AutoSize = true;
            labelFullName.Location = new Point(12, 204);
            labelFullName.Name = "labelFullName";
            labelFullName.Size = new Size(107, 30);
            labelFullName.TabIndex = 22;
            labelFullName.Text = "Full Name";
            // 
            // txtStudentFullName
            // 
            txtStudentFullName.Location = new Point(12, 237);
            txtStudentFullName.Name = "txtStudentFullName";
            txtStudentFullName.Size = new Size(294, 35);
            txtStudentFullName.TabIndex = 23;
            // 
            // labelAge
            // 
            labelAge.AutoSize = true;
            labelAge.Location = new Point(312, 204);
            labelAge.Name = "labelAge";
            labelAge.Size = new Size(50, 30);
            labelAge.TabIndex = 24;
            labelAge.Text = "Age";
            // 
            // txtStudentAge
            // 
            txtStudentAge.Location = new Point(312, 237);
            txtStudentAge.Name = "txtStudentAge";
            txtStudentAge.Size = new Size(86, 35);
            txtStudentAge.TabIndex = 25;
            // 
            // labelSex
            // 
            labelSex.AutoSize = true;
            labelSex.Location = new Point(404, 204);
            labelSex.Name = "labelSex";
            labelSex.Size = new Size(45, 30);
            labelSex.TabIndex = 26;
            labelSex.Text = "Sex";
            // 
            // cmbStudentSex
            // 
            cmbStudentSex.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStudentSex.FormattingEnabled = true;
            cmbStudentSex.Items.AddRange(new object[] { "Male", "Female" });
            cmbStudentSex.Location = new Point(404, 236);
            cmbStudentSex.Name = "cmbStudentSex";
            cmbStudentSex.Size = new Size(114, 38);
            cmbStudentSex.TabIndex = 27;
            // 
            // btnAddStudent
            // 
            btnAddStudent.Location = new Point(524, 234);
            btnAddStudent.Name = "btnAddStudent";
            btnAddStudent.Size = new Size(124, 40);
            btnAddStudent.TabIndex = 28;
            btnAddStudent.Text = "Add Student";
            btnAddStudent.UseVisualStyleBackColor = true;
            btnAddStudent.Click += btnAddStudent_Click;
            // 
            // txtStudentSensorSearch
            // 
            txtStudentSensorSearch.Location = new Point(1086, 541);
            txtStudentSensorSearch.Name = "txtStudentSensorSearch";
            txtStudentSensorSearch.PlaceholderText = "Search";
            txtStudentSensorSearch.Size = new Size(120, 35);
            txtStudentSensorSearch.TabIndex = 29;
            // 
            // cmbStudentGestureFilter
            // 
            cmbStudentGestureFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStudentGestureFilter.FormattingEnabled = true;
            cmbStudentGestureFilter.Location = new Point(1212, 540);
            cmbStudentGestureFilter.Name = "cmbStudentGestureFilter";
            cmbStudentGestureFilter.Size = new Size(200, 38);
            cmbStudentGestureFilter.TabIndex = 30;
            // 
            // cmbStudentSensorWindow
            // 
            cmbStudentSensorWindow.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStudentSensorWindow.FormattingEnabled = true;
            cmbStudentSensorWindow.Location = new Point(1435, 541);
            cmbStudentSensorWindow.Name = "cmbStudentSensorWindow";
            cmbStudentSensorWindow.Size = new Size(181, 38);
            cmbStudentSensorWindow.TabIndex = 31;
            // 
            // chkStudentSensorAlertOnly
            // 
            chkStudentSensorAlertOnly.AutoSize = true;
            chkStudentSensorAlertOnly.Location = new Point(1086, 593);
            chkStudentSensorAlertOnly.Name = "chkStudentSensorAlertOnly";
            chkStudentSensorAlertOnly.Size = new Size(83, 34);
            chkStudentSensorAlertOnly.TabIndex = 32;
            chkStudentSensorAlertOnly.Text = "Alert";
            chkStudentSensorAlertOnly.UseVisualStyleBackColor = true;
            // 
            // btnStudentFilterClear
            // 
            btnStudentFilterClear.Location = new Point(1172, 588);
            btnStudentFilterClear.Name = "btnStudentFilterClear";
            btnStudentFilterClear.Size = new Size(78, 43);
            btnStudentFilterClear.TabIndex = 33;
            btnStudentFilterClear.Text = "Clear";
            btnStudentFilterClear.UseVisualStyleBackColor = true;
            // 
            // dgvStudents
            // 
            dgvStudents.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvStudents.Location = new Point(12, 325);
            dgvStudents.Name = "dgvStudents";
            dgvStudents.RowHeadersWidth = 72;
            dgvStudents.Size = new Size(442, 272);
            dgvStudents.TabIndex = 34;
            dgvStudents.SelectionChanged += dgvStudents_SelectionChanged;
            // 
            // lblSelectedStudent
            // 
            lblSelectedStudent.Location = new Point(12, 652);
            lblSelectedStudent.Name = "lblSelectedStudent";
            lblSelectedStudent.Size = new Size(1001, 52);
            lblSelectedStudent.TabIndex = 35;
            lblSelectedStudent.Text = "Selected Student:";
            // 
            // lblConnectionStatus
            // 
            lblConnectionStatus.Location = new Point(12, 720);
            lblConnectionStatus.Name = "lblConnectionStatus";
            lblConnectionStatus.Size = new Size(1001, 52);
            lblConnectionStatus.TabIndex = 36;
            lblConnectionStatus.Text = "Connection:";
            // 
            // lblAlertHistoryTitle
            // 
            lblAlertHistoryTitle.AutoSize = true;
            lblAlertHistoryTitle.Location = new Point(1086, 642);
            lblAlertHistoryTitle.Name = "lblAlertHistoryTitle";
            lblAlertHistoryTitle.Size = new Size(128, 30);
            lblAlertHistoryTitle.TabIndex = 37;
            lblAlertHistoryTitle.Text = "Alert History";
            // 
            // cmbAlertHistoryStatus
            // 
            cmbAlertHistoryStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAlertHistoryStatus.FormattingEnabled = true;
            cmbAlertHistoryStatus.Location = new Point(1220, 639);
            cmbAlertHistoryStatus.Name = "cmbAlertHistoryStatus";
            cmbAlertHistoryStatus.Size = new Size(192, 38);
            cmbAlertHistoryStatus.TabIndex = 38;
            // 
            // btnAlertHistoryRefresh
            // 
            btnAlertHistoryRefresh.Location = new Point(1420, 637);
            btnAlertHistoryRefresh.Name = "btnAlertHistoryRefresh";
            btnAlertHistoryRefresh.Size = new Size(100, 40);
            btnAlertHistoryRefresh.TabIndex = 39;
            btnAlertHistoryRefresh.Text = "Refresh";
            btnAlertHistoryRefresh.UseVisualStyleBackColor = true;
            // 
            // lblAlertHistoryCount
            // 
            lblAlertHistoryCount.Location = new Point(1526, 637);
            lblAlertHistoryCount.Name = "lblAlertHistoryCount";
            lblAlertHistoryCount.Size = new Size(51, 40);
            lblAlertHistoryCount.TabIndex = 40;
            lblAlertHistoryCount.Text = "0";
            lblAlertHistoryCount.TextAlign = ContentAlignment.MiddleRight;
            // 
            // dgvAlertHistory
            // 
            dgvAlertHistory.AllowUserToAddRows = false;
            dgvAlertHistory.AllowUserToDeleteRows = false;
            dgvAlertHistory.AllowUserToResizeRows = false;
            dgvAlertHistory.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvAlertHistory.Location = new Point(1086, 801);
            dgvAlertHistory.MultiSelect = false;
            dgvAlertHistory.Name = "dgvAlertHistory";
            dgvAlertHistory.ReadOnly = true;
            dgvAlertHistory.RowHeadersVisible = false;
            dgvAlertHistory.RowHeadersWidth = 72;
            dgvAlertHistory.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAlertHistory.Size = new Size(530, 346);
            dgvAlertHistory.TabIndex = 41;
            // 
            // TeacherDashboard
            // 
            AutoScaleDimensions = new SizeF(12F, 30F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            AutoScrollMinSize = new Size(0, 1112);
            ClientSize = new Size(1632, 1226);
            Controls.Add(dgvAlertHistory);
            Controls.Add(lblCameraPreviewStatus);
            Controls.Add(picCameraPreview);
            Controls.Add(lblCameraPreviewTitle);
            Controls.Add(lblAlertHistoryCount);
            Controls.Add(btnAlertHistoryRefresh);
            Controls.Add(cmbAlertHistoryStatus);
            Controls.Add(lblAlertHistoryTitle);
            Controls.Add(lblConnectionStatus);
            Controls.Add(lblSelectedStudent);
            Controls.Add(dgvStudents);
            Controls.Add(btnStudentFilterClear);
            Controls.Add(chkStudentSensorAlertOnly);
            Controls.Add(cmbStudentSensorWindow);
            Controls.Add(cmbStudentGestureFilter);
            Controls.Add(txtStudentSensorSearch);
            Controls.Add(btnAddStudent);
            Controls.Add(cmbStudentSex);
            Controls.Add(labelSex);
            Controls.Add(txtStudentAge);
            Controls.Add(labelAge);
            Controls.Add(txtStudentFullName);
            Controls.Add(labelFullName);
            Controls.Add(lblStudentHeader);
            Controls.Add(pnlStudentConfidenceTrend);
            Controls.Add(pnlStudentVitalsTrend);
            Controls.Add(lblKpiStudentAlerts);
            Controls.Add(lblKpiGestureQuality);
            Controls.Add(lblKpiLatestGesture);
            Controls.Add(lblKpiStudentReadings);
            Controls.Add(chkDarkTheme);
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
            Name = "TeacherDashboard";
            Text = "Teacher Dashboard";
            Load += TeacherDashboard_Load;
            ((System.ComponentModel.ISupportInitialize)dgvSensorReadings).EndInit();
            ((System.ComponentModel.ISupportInitialize)picCameraPreview).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvStudents).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvAlertHistory).EndInit();
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
        private Label lblCameraPreviewTitle;
        private PictureBox picCameraPreview;
        private Label lblCameraPreviewStatus;
        private CheckBox chkDarkTheme;
        private Label lblKpiStudentReadings;
        private Label lblKpiLatestGesture;
        private Label lblKpiGestureQuality;
        private Label lblKpiStudentAlerts;
        private Panel pnlStudentVitalsTrend;
        private Panel pnlStudentConfidenceTrend;
        private Label lblStudentHeader;
        private Label labelFullName;
        private TextBox txtStudentFullName;
        private Label labelAge;
        private TextBox txtStudentAge;
        private Label labelSex;
        private ComboBox cmbStudentSex;
        private Button btnAddStudent;
        private TextBox txtStudentSensorSearch;
        private ComboBox cmbStudentGestureFilter;
        private ComboBox cmbStudentSensorWindow;
        private CheckBox chkStudentSensorAlertOnly;
        private Button btnStudentFilterClear;
        private DataGridView dgvStudents;
        private Label lblSelectedStudent;
        private Label lblConnectionStatus;
        private Label lblAlertHistoryTitle;
        private ComboBox cmbAlertHistoryStatus;
        private Button btnAlertHistoryRefresh;
        private Label lblAlertHistoryCount;
        private DataGridView dgvAlertHistory;
    }
}









