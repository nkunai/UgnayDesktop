namespace UgnayDesktop.Forms
{
    partial class AdminDashboard
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

        private void InitializeComponent()
        {
            labelTeachers = new Label();
            dgvTeachers = new DataGridView();
            labelStudents = new Label();
            dgvStudents = new DataGridView();
            label1 = new Label();
            txtFullName = new TextBox();
            txtNewUsername = new TextBox();
            label2 = new Label();
            txtNewPassword = new TextBox();
            label3 = new Label();
            label4 = new Label();
            cmbRole = new ComboBox();
            lblTeacherPhone = new Label();
            txtTeacherPhone = new TextBox();
            lblStudentAge = new Label();
            txtStudentAge = new TextBox();
            lblStudentSex = new Label();
            cmbStudentSex = new ComboBox();
            lblStudentDeviceId = new Label();
            txtStudentDeviceId = new TextBox();
            btnAddUser = new Button();
            btnUpdateUser = new Button();
            btnDeleteUser = new Button();
            lblSelectedUser = new Label();
            btnLogout = new Button();
            btnMqttTest = new Button();
            dgvSensorReadings = new DataGridView();
            lblDecisionStatus = new Label();
            ((System.ComponentModel.ISupportInitialize)dgvTeachers).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvStudents).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvSensorReadings).BeginInit();
            SuspendLayout();
            // 
            // labelTeachers
            // 
            labelTeachers.AutoSize = true;
            labelTeachers.Location = new Point(12, 62);
            labelTeachers.Name = "labelTeachers";
            labelTeachers.Size = new Size(93, 30);
            labelTeachers.TabIndex = 0;
            labelTeachers.Text = "Teachers";
            // 
            // dgvTeachers
            // 
            dgvTeachers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTeachers.Location = new Point(12, 95);
            dgvTeachers.Name = "dgvTeachers";
            dgvTeachers.RowHeadersWidth = 72;
            dgvTeachers.Size = new Size(520, 220);
            dgvTeachers.TabIndex = 1;
            dgvTeachers.SelectionChanged += dgvTeachers_SelectionChanged;
            // 
            // labelStudents
            // 
            labelStudents.AutoSize = true;
            labelStudents.Location = new Point(556, 62);
            labelStudents.Name = "labelStudents";
            labelStudents.Size = new Size(93, 30);
            labelStudents.TabIndex = 2;
            labelStudents.Text = "Students";
            // 
            // dgvStudents
            // 
            dgvStudents.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvStudents.Location = new Point(556, 95);
            dgvStudents.Name = "dgvStudents";
            dgvStudents.RowHeadersWidth = 72;
            dgvStudents.Size = new Size(520, 220);
            dgvStudents.TabIndex = 3;
            dgvStudents.SelectionChanged += dgvStudents_SelectionChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 338);
            label1.Name = "label1";
            label1.Size = new Size(107, 30);
            label1.TabIndex = 4;
            label1.Text = "Full Name";
            // 
            // txtFullName
            // 
            txtFullName.Location = new Point(12, 371);
            txtFullName.Name = "txtFullName";
            txtFullName.Size = new Size(268, 35);
            txtFullName.TabIndex = 5;
            // 
            // txtNewUsername
            // 
            txtNewUsername.Location = new Point(12, 448);
            txtNewUsername.Name = "txtNewUsername";
            txtNewUsername.Size = new Size(268, 35);
            txtNewUsername.TabIndex = 7;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 415);
            label2.Name = "label2";
            label2.Size = new Size(106, 30);
            label2.TabIndex = 6;
            label2.Text = "Username";
            // 
            // txtNewPassword
            // 
            txtNewPassword.Location = new Point(12, 529);
            txtNewPassword.Name = "txtNewPassword";
            txtNewPassword.Size = new Size(268, 35);
            txtNewPassword.TabIndex = 9;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 496);
            label3.Name = "label3";
            label3.Size = new Size(193, 30);
            label3.TabIndex = 8;
            label3.Text = "Password (optional)";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 576);
            label4.Name = "label4";
            label4.Size = new Size(53, 30);
            label4.TabIndex = 10;
            label4.Text = "Role";
            // 
            // cmbRole
            // 
            cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRole.FormattingEnabled = true;
            cmbRole.Items.AddRange(new object[] { "Admin", "Teacher", "Student" });
            cmbRole.Location = new Point(12, 609);
            cmbRole.Name = "cmbRole";
            cmbRole.Size = new Size(268, 38);
            cmbRole.TabIndex = 11;
            cmbRole.SelectedIndexChanged += cmbRole_SelectedIndexChanged;
            // 
            // lblTeacherPhone
            // 
            lblTeacherPhone.AutoSize = true;
            lblTeacherPhone.Location = new Point(12, 650);
            lblTeacherPhone.Name = "lblTeacherPhone";
            lblTeacherPhone.Size = new Size(156, 30);
            lblTeacherPhone.TabIndex = 12;
            lblTeacherPhone.Text = "Teacher Phone";
            // 
            // txtTeacherPhone
            // 
            txtTeacherPhone.Location = new Point(12, 683);
            txtTeacherPhone.Name = "txtTeacherPhone";
            txtTeacherPhone.Size = new Size(268, 35);
            txtTeacherPhone.TabIndex = 13;
            // 
            // lblStudentAge
            // 
            lblStudentAge.AutoSize = true;
            lblStudentAge.Location = new Point(12, 721);
            lblStudentAge.Name = "lblStudentAge";
            lblStudentAge.Size = new Size(127, 30);
            lblStudentAge.TabIndex = 14;
            lblStudentAge.Text = "Student Age";
            // 
            // txtStudentAge
            // 
            txtStudentAge.Location = new Point(12, 754);
            txtStudentAge.Name = "txtStudentAge";
            txtStudentAge.Size = new Size(126, 35);
            txtStudentAge.TabIndex = 15;
            // 
            // lblStudentSex
            // 
            lblStudentSex.AutoSize = true;
            lblStudentSex.Location = new Point(149, 721);
            lblStudentSex.Name = "lblStudentSex";
            lblStudentSex.Size = new Size(122, 30);
            lblStudentSex.TabIndex = 16;
            lblStudentSex.Text = "Student Sex";
            // 
            // cmbStudentSex
            // 
            cmbStudentSex.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStudentSex.FormattingEnabled = true;
            cmbStudentSex.Items.AddRange(new object[] { "Male", "Female" });
            cmbStudentSex.Location = new Point(149, 754);
            cmbStudentSex.Name = "cmbStudentSex";
            cmbStudentSex.Size = new Size(131, 38);
            cmbStudentSex.TabIndex = 17;
            // 
            // lblStudentDeviceId
            // 
            lblStudentDeviceId.AutoSize = true;
            lblStudentDeviceId.Location = new Point(12, 795);
            lblStudentDeviceId.Name = "lblStudentDeviceId";
            lblStudentDeviceId.Size = new Size(179, 30);
            lblStudentDeviceId.TabIndex = 18;
            lblStudentDeviceId.Text = "Student Device ID";
            // 
            // txtStudentDeviceId
            // 
            txtStudentDeviceId.Location = new Point(12, 828);
            txtStudentDeviceId.Name = "txtStudentDeviceId";
            txtStudentDeviceId.Size = new Size(268, 35);
            txtStudentDeviceId.TabIndex = 19;
            // 
            // btnAddUser
            // 
            btnAddUser.Location = new Point(12, 868);
            btnAddUser.Name = "btnAddUser";
            btnAddUser.Size = new Size(131, 40);
            btnAddUser.TabIndex = 20;
            btnAddUser.Text = "Add User";
            btnAddUser.UseVisualStyleBackColor = true;
            btnAddUser.Click += btnAddUser_Click;
            // 
            // btnUpdateUser
            // 
            btnUpdateUser.Location = new Point(149, 868);
            btnUpdateUser.Name = "btnUpdateUser";
            btnUpdateUser.Size = new Size(131, 40);
            btnUpdateUser.TabIndex = 21;
            btnUpdateUser.Text = "Update";
            btnUpdateUser.UseVisualStyleBackColor = true;
            btnUpdateUser.Click += btnUpdateUser_Click;
            // 
            // btnDeleteUser
            // 
            btnDeleteUser.Location = new Point(149, 914);
            btnDeleteUser.Name = "btnDeleteUser";
            btnDeleteUser.Size = new Size(131, 40);
            btnDeleteUser.TabIndex = 22;
            btnDeleteUser.Text = "Delete User";
            btnDeleteUser.UseVisualStyleBackColor = true;
            btnDeleteUser.Click += btnDeleteUser_Click;
            // 
            // lblSelectedUser
            // 
            lblSelectedUser.AutoSize = true;
            lblSelectedUser.Location = new Point(12, 914);
            lblSelectedUser.Name = "lblSelectedUser";
            lblSelectedUser.Size = new Size(136, 30);
            lblSelectedUser.TabIndex = 23;
            lblSelectedUser.Text = "Selected user";
            // 
            // btnLogout
            // 
            btnLogout.Location = new Point(945, 12);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(131, 40);
            btnLogout.TabIndex = 24;
            btnLogout.Text = "Logout";
            btnLogout.UseVisualStyleBackColor = true;
            btnLogout.Click += btnLogout_Click;
            // 
            // btnMqttTest
            // 
            btnMqttTest.Location = new Point(339, 969);
            btnMqttTest.Name = "btnMqttTest";
            btnMqttTest.Size = new Size(131, 40);
            btnMqttTest.TabIndex = 25;
            btnMqttTest.Text = "UDP 5005 Active";
            btnMqttTest.UseVisualStyleBackColor = true;
            btnMqttTest.Click += btnMqttTest_Click;
            // 
            // dgvSensorReadings
            // 
            dgvSensorReadings.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvSensorReadings.Location = new Point(339, 338);
            dgvSensorReadings.Name = "dgvSensorReadings";
            dgvSensorReadings.RowHeadersWidth = 72;
            dgvSensorReadings.Size = new Size(737, 595);
            dgvSensorReadings.TabIndex = 26;
            // 
            // lblDecisionStatus
            // 
            lblDecisionStatus.AutoSize = true;
            lblDecisionStatus.Location = new Point(339, 936);
            lblDecisionStatus.Name = "lblDecisionStatus";
            lblDecisionStatus.Size = new Size(92, 30);
            lblDecisionStatus.TabIndex = 27;
            lblDecisionStatus.Text = "Decision";
            // 
            // AdminDashboard
            // 
            AutoScaleDimensions = new SizeF(12F, 30F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1088, 1024);
            Controls.Add(lblDecisionStatus);
            Controls.Add(dgvSensorReadings);
            Controls.Add(btnMqttTest);
            Controls.Add(btnLogout);
            Controls.Add(lblSelectedUser);
            Controls.Add(btnDeleteUser);
            Controls.Add(btnUpdateUser);
            Controls.Add(btnAddUser);
            Controls.Add(txtStudentDeviceId);
            Controls.Add(lblStudentDeviceId);
            Controls.Add(cmbStudentSex);
            Controls.Add(lblStudentSex);
            Controls.Add(txtStudentAge);
            Controls.Add(lblStudentAge);
            Controls.Add(txtTeacherPhone);
            Controls.Add(lblTeacherPhone);
            Controls.Add(cmbRole);
            Controls.Add(label4);
            Controls.Add(txtNewPassword);
            Controls.Add(label3);
            Controls.Add(txtNewUsername);
            Controls.Add(label2);
            Controls.Add(txtFullName);
            Controls.Add(label1);
            Controls.Add(dgvStudents);
            Controls.Add(labelStudents);
            Controls.Add(dgvTeachers);
            Controls.Add(labelTeachers);
            Name = "AdminDashboard";
            Text = "Admin Dashboard";
            ((System.ComponentModel.ISupportInitialize)dgvTeachers).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvStudents).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvSensorReadings).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelTeachers;
        private DataGridView dgvTeachers;
        private Label labelStudents;
        private DataGridView dgvStudents;
        private Label label1;
        private TextBox txtFullName;
        private TextBox txtNewUsername;
        private Label label2;
        private TextBox txtNewPassword;
        private Label label3;
        private Label label4;
        private ComboBox cmbRole;
        private Label lblTeacherPhone;
        private TextBox txtTeacherPhone;
        private Label lblStudentAge;
        private TextBox txtStudentAge;
        private Label lblStudentSex;
        private ComboBox cmbStudentSex;
        private Label lblStudentDeviceId;
        private TextBox txtStudentDeviceId;
        private Button btnAddUser;
        private Button btnUpdateUser;
        private Button btnDeleteUser;
        private Label lblSelectedUser;
        private Button btnLogout;
        private Button btnMqttTest;
        private DataGridView dgvSensorReadings;
        private Label lblDecisionStatus;
    }
}

