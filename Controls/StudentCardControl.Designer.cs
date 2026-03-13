namespace UgnayDesktop.Controls
{
    partial class StudentCardControl
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

        #region Component Designer generated code

        private void InitializeComponent()
        {
            panelAccent = new Panel();
            tableLayoutPanelMain = new TableLayoutPanel();
            panelHeader = new Panel();
            lblHeading = new Label();
            lblSeverity = new Label();
            lblConnectionValue = new Label();
            lblLastSeenValue = new Label();
            tableMetrics = new TableLayoutPanel();
            lblHeartRateCaption = new Label();
            lblHeartRateValue = new Label();
            lblSweatnessCaption = new Label();
            lblSweatnessValue = new Label();
            lblTemperatureCaption = new Label();
            lblTemperatureValue = new Label();
            lblGestureCaption = new Label();
            lblGestureValue = new Label();
            lblDeviceIdCaption = new Label();
            lblDeviceIdValue = new Label();
            panelActions = new FlowLayoutPanel();
            btnEdit = new Button();
            btnDetails = new Button();
            btnAlert = new Button();
            tableLayoutPanelMain.SuspendLayout();
            panelHeader.SuspendLayout();
            tableMetrics.SuspendLayout();
            panelActions.SuspendLayout();
            SuspendLayout();
            // panelAccent
            panelAccent.BackColor = Color.FromArgb(155, 163, 172);
            panelAccent.Dock = DockStyle.Left;
            panelAccent.Location = new Point(0, 0);
            panelAccent.Name = "panelAccent";
            panelAccent.Size = new Size(8, 366);
            panelAccent.TabIndex = 0;
            // tableLayoutPanelMain
            tableLayoutPanelMain.ColumnCount = 1;
            tableLayoutPanelMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanelMain.Controls.Add(panelHeader, 0, 0);
            tableLayoutPanelMain.Controls.Add(lblConnectionValue, 0, 1);
            tableLayoutPanelMain.Controls.Add(lblLastSeenValue, 0, 2);
            tableLayoutPanelMain.Controls.Add(tableMetrics, 0, 3);
            tableLayoutPanelMain.Controls.Add(panelActions, 0, 4);
            tableLayoutPanelMain.Dock = DockStyle.Fill;
            tableLayoutPanelMain.Location = new Point(8, 0);
            tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            tableLayoutPanelMain.Padding = new Padding(16, 14, 16, 14);
            tableLayoutPanelMain.RowCount = 5;
            tableLayoutPanelMain.RowStyles.Add(new RowStyle());
            tableLayoutPanelMain.RowStyles.Add(new RowStyle());
            tableLayoutPanelMain.RowStyles.Add(new RowStyle());
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanelMain.RowStyles.Add(new RowStyle());
            tableLayoutPanelMain.Size = new Size(400, 366);
            tableLayoutPanelMain.TabIndex = 1;
            // panelHeader
            panelHeader.Controls.Add(lblHeading);
            panelHeader.Controls.Add(lblSeverity);
            panelHeader.Dock = DockStyle.Fill;
            panelHeader.Location = new Point(19, 17);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(362, 53);
            panelHeader.TabIndex = 0;
            // lblHeading
            lblHeading.AutoSize = true;
            lblHeading.Font = new Font("Segoe UI", 13F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblHeading.Location = new Point(0, 6);
            lblHeading.MaximumSize = new Size(228, 0);
            lblHeading.Name = "lblHeading";
            lblHeading.Size = new Size(94, 36);
            lblHeading.TabIndex = 0;
            lblHeading.Text = "Student";
            // lblSeverity
            lblSeverity.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblSeverity.AutoSize = true;
            lblSeverity.BackColor = Color.FromArgb(231, 236, 241);
            lblSeverity.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblSeverity.ForeColor = Color.FromArgb(95, 105, 115);
            lblSeverity.Location = new Point(261, 8);
            lblSeverity.Name = "lblSeverity";
            lblSeverity.Padding = new Padding(10, 6, 10, 6);
            lblSeverity.Size = new Size(89, 37);
            lblSeverity.TabIndex = 1;
            lblSeverity.Text = "No Data";
            // lblConnectionValue
            lblConnectionValue.AutoSize = true;
            lblConnectionValue.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblConnectionValue.ForeColor = Color.DimGray;
            lblConnectionValue.Location = new Point(19, 73);
            lblConnectionValue.Margin = new Padding(3, 0, 3, 4);
            lblConnectionValue.Name = "lblConnectionValue";
            lblConnectionValue.Size = new Size(211, 25);
            lblConnectionValue.TabIndex = 1;
            lblConnectionValue.Text = "Status: waiting for sensor data";
            // lblLastSeenValue
            lblLastSeenValue.AutoSize = true;
            lblLastSeenValue.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblLastSeenValue.ForeColor = Color.FromArgb(90, 101, 115);
            lblLastSeenValue.Location = new Point(19, 102);
            lblLastSeenValue.Margin = new Padding(3, 0, 3, 12);
            lblLastSeenValue.Name = "lblLastSeenValue";
            lblLastSeenValue.Size = new Size(164, 25);
            lblLastSeenValue.TabIndex = 2;
            lblLastSeenValue.Text = "Last reading: no data yet";
            // tableMetrics
            tableMetrics.ColumnCount = 2;
            tableMetrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableMetrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableMetrics.Controls.Add(lblHeartRateCaption, 0, 0);
            tableMetrics.Controls.Add(lblHeartRateValue, 0, 1);
            tableMetrics.Controls.Add(lblSweatnessCaption, 1, 0);
            tableMetrics.Controls.Add(lblSweatnessValue, 1, 1);
            tableMetrics.Controls.Add(lblTemperatureCaption, 0, 2);
            tableMetrics.Controls.Add(lblTemperatureValue, 0, 3);
            tableMetrics.Controls.Add(lblGestureCaption, 1, 2);
            tableMetrics.Controls.Add(lblGestureValue, 1, 3);
            tableMetrics.Controls.Add(lblDeviceIdCaption, 0, 4);
            tableMetrics.Controls.Add(lblDeviceIdValue, 0, 5);
            tableMetrics.Dock = DockStyle.Fill;
            tableMetrics.Location = new Point(19, 142);
            tableMetrics.Name = "tableMetrics";
            tableMetrics.RowCount = 6;
            tableMetrics.RowStyles.Add(new RowStyle());
            tableMetrics.RowStyles.Add(new RowStyle());
            tableMetrics.RowStyles.Add(new RowStyle());
            tableMetrics.RowStyles.Add(new RowStyle());
            tableMetrics.RowStyles.Add(new RowStyle());
            tableMetrics.RowStyles.Add(new RowStyle());
            tableMetrics.Size = new Size(362, 155);
            tableMetrics.TabIndex = 3;
            // lblHeartRateCaption
            lblHeartRateCaption.AutoSize = true;
            lblHeartRateCaption.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblHeartRateCaption.ForeColor = Color.FromArgb(90, 101, 115);
            lblHeartRateCaption.Location = new Point(0, 0);
            lblHeartRateCaption.Margin = new Padding(0, 0, 0, 2);
            lblHeartRateCaption.Name = "lblHeartRateCaption";
            lblHeartRateCaption.Size = new Size(104, 25);
            lblHeartRateCaption.TabIndex = 0;
            lblHeartRateCaption.Text = "Heart rate";
            // lblHeartRateValue
            lblHeartRateValue.AutoSize = true;
            lblHeartRateValue.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblHeartRateValue.ForeColor = Color.FromArgb(181, 39, 39);
            lblHeartRateValue.Location = new Point(0, 27);
            lblHeartRateValue.Margin = new Padding(0, 0, 0, 10);
            lblHeartRateValue.Name = "lblHeartRateValue";
            lblHeartRateValue.Size = new Size(35, 45);
            lblHeartRateValue.TabIndex = 1;
            lblHeartRateValue.Text = "--";
            // lblSweatnessCaption
            lblSweatnessCaption.AutoSize = true;
            lblSweatnessCaption.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblSweatnessCaption.ForeColor = Color.FromArgb(90, 101, 115);
            lblSweatnessCaption.Location = new Point(181, 0);
            lblSweatnessCaption.Margin = new Padding(0, 0, 0, 2);
            lblSweatnessCaption.Name = "lblSweatnessCaption";
            lblSweatnessCaption.Size = new Size(98, 25);
            lblSweatnessCaption.TabIndex = 2;
            lblSweatnessCaption.Text = "Sweatness";
            // lblSweatnessValue
            lblSweatnessValue.AutoSize = true;
            lblSweatnessValue.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblSweatnessValue.ForeColor = Color.FromArgb(3, 102, 214);
            lblSweatnessValue.Location = new Point(181, 27);
            lblSweatnessValue.Margin = new Padding(0, 0, 0, 10);
            lblSweatnessValue.Name = "lblSweatnessValue";
            lblSweatnessValue.Size = new Size(35, 45);
            lblSweatnessValue.TabIndex = 3;
            lblSweatnessValue.Text = "--";
            // lblTemperatureCaption
            lblTemperatureCaption.AutoSize = true;
            lblTemperatureCaption.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTemperatureCaption.ForeColor = Color.FromArgb(90, 101, 115);
            lblTemperatureCaption.Location = new Point(0, 82);
            lblTemperatureCaption.Margin = new Padding(0, 0, 0, 2);
            lblTemperatureCaption.Name = "lblTemperatureCaption";
            lblTemperatureCaption.Size = new Size(111, 25);
            lblTemperatureCaption.TabIndex = 4;
            lblTemperatureCaption.Text = "Temperature";
            // lblTemperatureValue
            lblTemperatureValue.AutoSize = true;
            lblTemperatureValue.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTemperatureValue.ForeColor = Color.FromArgb(190, 85, 0);
            lblTemperatureValue.Location = new Point(0, 109);
            lblTemperatureValue.Margin = new Padding(0, 0, 0, 10);
            lblTemperatureValue.Name = "lblTemperatureValue";
            lblTemperatureValue.Size = new Size(35, 45);
            lblTemperatureValue.TabIndex = 5;
            lblTemperatureValue.Text = "--";
            // lblGestureCaption
            lblGestureCaption.AutoSize = true;
            lblGestureCaption.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblGestureCaption.ForeColor = Color.FromArgb(90, 101, 115);
            lblGestureCaption.Location = new Point(181, 82);
            lblGestureCaption.Margin = new Padding(0, 0, 0, 2);
            lblGestureCaption.Name = "lblGestureCaption";
            lblGestureCaption.Size = new Size(74, 25);
            lblGestureCaption.TabIndex = 6;
            lblGestureCaption.Text = "Gesture";
            // lblGestureValue
            lblGestureValue.AutoSize = true;
            lblGestureValue.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblGestureValue.Location = new Point(181, 109);
            lblGestureValue.Margin = new Padding(0, 0, 0, 10);
            lblGestureValue.MaximumSize = new Size(168, 0);
            lblGestureValue.Name = "lblGestureValue";
            lblGestureValue.Size = new Size(147, 28);
            lblGestureValue.TabIndex = 7;
            lblGestureValue.Text = "Waiting for data";
            // lblDeviceIdCaption
            lblDeviceIdCaption.AutoSize = true;
            lblDeviceIdCaption.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblDeviceIdCaption.ForeColor = Color.FromArgb(90, 101, 115);
            lblDeviceIdCaption.Location = new Point(0, 164);
            lblDeviceIdCaption.Margin = new Padding(0, 0, 0, 2);
            lblDeviceIdCaption.Name = "lblDeviceIdCaption";
            lblDeviceIdCaption.Size = new Size(79, 25);
            lblDeviceIdCaption.TabIndex = 8;
            lblDeviceIdCaption.Text = "Device ID";
            // lblDeviceIdValue
            lblDeviceIdValue.AutoSize = true;
            lblDeviceIdValue.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblDeviceIdValue.ForeColor = Color.FromArgb(90, 101, 115);
            lblDeviceIdValue.Location = new Point(0, 191);
            lblDeviceIdValue.Margin = new Padding(0);
            lblDeviceIdValue.MaximumSize = new Size(344, 0);
            lblDeviceIdValue.Name = "lblDeviceIdValue";
            lblDeviceIdValue.Size = new Size(103, 25);
            lblDeviceIdValue.TabIndex = 9;
            lblDeviceIdValue.Text = "Not assigned";
            tableMetrics.SetColumnSpan(lblDeviceIdValue, 2);
            tableMetrics.SetColumnSpan(lblDeviceIdCaption, 2);
            // panelActions
            panelActions.AutoSize = true;
            panelActions.Controls.Add(btnEdit);
            panelActions.Controls.Add(btnDetails);
            panelActions.Controls.Add(btnAlert);
            panelActions.Dock = DockStyle.Fill;
            panelActions.Location = new Point(19, 303);
            panelActions.Name = "panelActions";
            panelActions.Size = new Size(362, 46);
            panelActions.TabIndex = 4;
            // btnEdit
            btnEdit.Location = new Point(0, 0);
            btnEdit.Margin = new Padding(0, 0, 8, 0);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(92, 38);
            btnEdit.TabIndex = 0;
            btnEdit.Text = "Edit";
            btnEdit.UseVisualStyleBackColor = true;
            // btnDetails
            btnDetails.Location = new Point(100, 0);
            btnDetails.Margin = new Padding(0, 0, 8, 0);
            btnDetails.Name = "btnDetails";
            btnDetails.Size = new Size(92, 38);
            btnDetails.TabIndex = 1;
            btnDetails.Text = "Details";
            btnDetails.UseVisualStyleBackColor = true;
            // btnAlert
            btnAlert.Location = new Point(200, 0);
            btnAlert.Margin = new Padding(0);
            btnAlert.Name = "btnAlert";
            btnAlert.Size = new Size(110, 38);
            btnAlert.TabIndex = 2;
            btnAlert.Text = "Send Alert";
            btnAlert.UseVisualStyleBackColor = true;
            // StudentCardControl
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(tableLayoutPanelMain);
            Controls.Add(panelAccent);
            Margin = new Padding(12);
            MinimumSize = new Size(410, 368);
            Name = "StudentCardControl";
            Size = new Size(408, 366);
            tableLayoutPanelMain.ResumeLayout(false);
            tableLayoutPanelMain.PerformLayout();
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            tableMetrics.ResumeLayout(false);
            tableMetrics.PerformLayout();
            panelActions.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panelAccent;
        private TableLayoutPanel tableLayoutPanelMain;
        private Panel panelHeader;
        private Label lblHeading;
        private Label lblSeverity;
        private Label lblConnectionValue;
        private Label lblLastSeenValue;
        private TableLayoutPanel tableMetrics;
        private Label lblHeartRateCaption;
        private Label lblHeartRateValue;
        private Label lblSweatnessCaption;
        private Label lblSweatnessValue;
        private Label lblTemperatureCaption;
        private Label lblTemperatureValue;
        private Label lblGestureCaption;
        private Label lblGestureValue;
        private Label lblDeviceIdCaption;
        private Label lblDeviceIdValue;
        private FlowLayoutPanel panelActions;
        private Button btnEdit;
        private Button btnDetails;
        private Button btnAlert;
    }
}
