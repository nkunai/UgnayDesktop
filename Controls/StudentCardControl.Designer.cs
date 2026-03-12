namespace UgnayDesktop.Controls
{
    partial class StudentCardControl
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tableLayoutPanelMain = new TableLayoutPanel();
            lblHeading = new Label();
            lblConnectionValue = new Label();
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
            tableLayoutPanelMain.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanelMain
            // 
            tableLayoutPanelMain.AutoSize = true;
            tableLayoutPanelMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanelMain.ColumnCount = 1;
            tableLayoutPanelMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanelMain.Controls.Add(lblHeading, 0, 0);
            tableLayoutPanelMain.Controls.Add(lblConnectionValue, 0, 1);
            tableLayoutPanelMain.Controls.Add(lblHeartRateCaption, 0, 2);
            tableLayoutPanelMain.Controls.Add(lblHeartRateValue, 0, 3);
            tableLayoutPanelMain.Controls.Add(lblSweatnessCaption, 0, 4);
            tableLayoutPanelMain.Controls.Add(lblSweatnessValue, 0, 5);
            tableLayoutPanelMain.Controls.Add(lblTemperatureCaption, 0, 6);
            tableLayoutPanelMain.Controls.Add(lblTemperatureValue, 0, 7);
            tableLayoutPanelMain.Controls.Add(lblGestureCaption, 0, 8);
            tableLayoutPanelMain.Controls.Add(lblGestureValue, 0, 9);
            tableLayoutPanelMain.Controls.Add(lblDeviceIdCaption, 0, 10);
            tableLayoutPanelMain.Controls.Add(lblDeviceIdValue, 0, 11);
            tableLayoutPanelMain.Dock = DockStyle.Fill;
            tableLayoutPanelMain.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
            tableLayoutPanelMain.Location = new Point(0, 0);
            tableLayoutPanelMain.Margin = new Padding(0);
            tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            tableLayoutPanelMain.Padding = new Padding(18, 16, 18, 16);
            tableLayoutPanelMain.RowCount = 12;
            tableLayoutPanelMain.RowStyles.Add(new RowStyle());
            tableLayoutPanelMain.RowStyles.Add(new RowStyle());
            tableLayoutPanelMain.RowStyles.Add(new RowStyle());
            tableLayoutPanelMain.RowStyles.Add(new RowStyle());
            tableLayoutPanelMain.RowStyles.Add(new RowStyle());
            tableLayoutPanelMain.RowStyles.Add(new RowStyle());
            tableLayoutPanelMain.RowStyles.Add(new RowStyle());
            tableLayoutPanelMain.RowStyles.Add(new RowStyle());
            tableLayoutPanelMain.RowStyles.Add(new RowStyle());
            tableLayoutPanelMain.RowStyles.Add(new RowStyle());
            tableLayoutPanelMain.RowStyles.Add(new RowStyle());
            tableLayoutPanelMain.RowStyles.Add(new RowStyle());
            tableLayoutPanelMain.Size = new Size(418, 478);
            tableLayoutPanelMain.TabIndex = 0;
            // 
            // lblHeading
            // 
            lblHeading.AutoSize = true;
            lblHeading.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblHeading.Location = new Point(18, 16);
            lblHeading.Margin = new Padding(0, 0, 0, 8);
            lblHeading.MaximumSize = new Size(382, 0);
            lblHeading.Name = "lblHeading";
            lblHeading.Size = new Size(94, 36);
            lblHeading.TabIndex = 0;
            lblHeading.Text = "Student";
            // 
            // lblConnectionValue
            // 
            lblConnectionValue.AutoSize = true;
            lblConnectionValue.Font = new Font("Segoe UI", 9F);
            lblConnectionValue.ForeColor = Color.DimGray;
            lblConnectionValue.Location = new Point(18, 60);
            lblConnectionValue.Margin = new Padding(0, 0, 0, 14);
            lblConnectionValue.MaximumSize = new Size(382, 0);
            lblConnectionValue.Name = "lblConnectionValue";
            lblConnectionValue.Size = new Size(211, 25);
            lblConnectionValue.TabIndex = 1;
            lblConnectionValue.Text = "Status: waiting for sensor data";
            // 
            // lblHeartRateCaption
            // 
            lblHeartRateCaption.AutoSize = true;
            lblHeartRateCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblHeartRateCaption.ForeColor = Color.DimGray;
            lblHeartRateCaption.Location = new Point(18, 99);
            lblHeartRateCaption.Margin = new Padding(0, 0, 0, 2);
            lblHeartRateCaption.MaximumSize = new Size(382, 0);
            lblHeartRateCaption.Name = "lblHeartRateCaption";
            lblHeartRateCaption.Size = new Size(116, 25);
            lblHeartRateCaption.TabIndex = 2;
            lblHeartRateCaption.Text = "Heart Rate BPM";
            // 
            // lblHeartRateValue
            // 
            lblHeartRateValue.AutoSize = true;
            lblHeartRateValue.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblHeartRateValue.ForeColor = Color.FromArgb(181, 39, 39);
            lblHeartRateValue.Location = new Point(18, 126);
            lblHeartRateValue.Margin = new Padding(0, 0, 0, 12);
            lblHeartRateValue.MaximumSize = new Size(382, 0);
            lblHeartRateValue.Name = "lblHeartRateValue";
            lblHeartRateValue.Size = new Size(43, 54);
            lblHeartRateValue.TabIndex = 3;
            lblHeartRateValue.Text = "--";
            // 
            // lblSweatnessCaption
            // 
            lblSweatnessCaption.AutoSize = true;
            lblSweatnessCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblSweatnessCaption.ForeColor = Color.DimGray;
            lblSweatnessCaption.Location = new Point(18, 192);
            lblSweatnessCaption.Margin = new Padding(0, 0, 0, 2);
            lblSweatnessCaption.MaximumSize = new Size(382, 0);
            lblSweatnessCaption.Name = "lblSweatnessCaption";
            lblSweatnessCaption.Size = new Size(124, 25);
            lblSweatnessCaption.TabIndex = 4;
            lblSweatnessCaption.Text = "Sweatness Level";
            // 
            // lblSweatnessValue
            // 
            lblSweatnessValue.AutoSize = true;
            lblSweatnessValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblSweatnessValue.ForeColor = Color.FromArgb(3, 102, 214);
            lblSweatnessValue.Location = new Point(18, 219);
            lblSweatnessValue.Margin = new Padding(0, 0, 0, 12);
            lblSweatnessValue.MaximumSize = new Size(382, 0);
            lblSweatnessValue.Name = "lblSweatnessValue";
            lblSweatnessValue.Size = new Size(39, 48);
            lblSweatnessValue.TabIndex = 5;
            lblSweatnessValue.Text = "--";
            // 
            // lblTemperatureCaption
            // 
            lblTemperatureCaption.AutoSize = true;
            lblTemperatureCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTemperatureCaption.ForeColor = Color.DimGray;
            lblTemperatureCaption.Location = new Point(18, 279);
            lblTemperatureCaption.Margin = new Padding(0, 0, 0, 2);
            lblTemperatureCaption.MaximumSize = new Size(382, 0);
            lblTemperatureCaption.Name = "lblTemperatureCaption";
            lblTemperatureCaption.Size = new Size(102, 25);
            lblTemperatureCaption.TabIndex = 6;
            lblTemperatureCaption.Text = "Temperature";
            // 
            // lblTemperatureValue
            // 
            lblTemperatureValue.AutoSize = true;
            lblTemperatureValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTemperatureValue.ForeColor = Color.FromArgb(190, 85, 0);
            lblTemperatureValue.Location = new Point(18, 306);
            lblTemperatureValue.Margin = new Padding(0, 0, 0, 12);
            lblTemperatureValue.MaximumSize = new Size(382, 0);
            lblTemperatureValue.Name = "lblTemperatureValue";
            lblTemperatureValue.Size = new Size(39, 48);
            lblTemperatureValue.TabIndex = 7;
            lblTemperatureValue.Text = "--";
            // 
            // lblGestureCaption
            // 
            lblGestureCaption.AutoSize = true;
            lblGestureCaption.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            lblGestureCaption.ForeColor = Color.DimGray;
            lblGestureCaption.Location = new Point(18, 366);
            lblGestureCaption.Margin = new Padding(0, 0, 0, 2);
            lblGestureCaption.MaximumSize = new Size(382, 0);
            lblGestureCaption.Name = "lblGestureCaption";
            lblGestureCaption.Size = new Size(130, 23);
            lblGestureCaption.TabIndex = 8;
            lblGestureCaption.Text = "Recognized Gesture";
            // 
            // lblGestureValue
            // 
            lblGestureValue.AutoSize = true;
            lblGestureValue.Font = new Font("Segoe UI", 10F);
            lblGestureValue.Location = new Point(18, 391);
            lblGestureValue.Margin = new Padding(0, 0, 0, 10);
            lblGestureValue.MaximumSize = new Size(382, 0);
            lblGestureValue.Name = "lblGestureValue";
            lblGestureValue.Size = new Size(191, 28);
            lblGestureValue.TabIndex = 9;
            lblGestureValue.Text = "Waiting for connection";
            // 
            // lblDeviceIdCaption
            // 
            lblDeviceIdCaption.AutoSize = true;
            lblDeviceIdCaption.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            lblDeviceIdCaption.ForeColor = Color.DimGray;
            lblDeviceIdCaption.Location = new Point(18, 429);
            lblDeviceIdCaption.Margin = new Padding(0, 0, 0, 2);
            lblDeviceIdCaption.MaximumSize = new Size(382, 0);
            lblDeviceIdCaption.Name = "lblDeviceIdCaption";
            lblDeviceIdCaption.Size = new Size(114, 23);
            lblDeviceIdCaption.TabIndex = 10;
            lblDeviceIdCaption.Text = "Glove Device ID";
            // 
            // lblDeviceIdValue
            // 
            lblDeviceIdValue.AutoSize = true;
            lblDeviceIdValue.Font = new Font("Segoe UI", 9F);
            lblDeviceIdValue.ForeColor = Color.DimGray;
            lblDeviceIdValue.Location = new Point(18, 454);
            lblDeviceIdValue.Margin = new Padding(0);
            lblDeviceIdValue.MaximumSize = new Size(382, 0);
            lblDeviceIdValue.Name = "lblDeviceIdValue";
            lblDeviceIdValue.Size = new Size(103, 24);
            lblDeviceIdValue.TabIndex = 11;
            lblDeviceIdValue.Text = "Not assigned";
            // 
            // StudentCardControl
            // 
            AutoScaleDimensions = new SizeF(12F, 30F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BackColor = Color.White;
            BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(tableLayoutPanelMain);
            Margin = new Padding(12);
            MinimumSize = new Size(420, 480);
            Name = "StudentCardControl";
            Size = new Size(420, 480);
            tableLayoutPanelMain.ResumeLayout(false);
            tableLayoutPanelMain.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel tableLayoutPanelMain;
        private Label lblHeading;
        private Label lblDeviceIdCaption;
        private Label lblDeviceIdValue;
        private Label lblGestureCaption;
        private Label lblGestureValue;
        private Label lblHeartRateCaption;
        private Label lblHeartRateValue;
        private Label lblSweatnessCaption;
        private Label lblSweatnessValue;
        private Label lblTemperatureCaption;
        private Label lblTemperatureValue;
        private Label lblConnectionValue;
    }
}
