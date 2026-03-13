using System.Drawing;
using System.Windows.Forms;

namespace UgnayDesktop.Forms;

internal sealed class StudentDetailsDialog : Form
{
    public StudentDetailsDialog(
        string studentName,
        string deviceId,
        string severity,
        string connection,
        string lastSeen,
        string heartRate,
        string sweatness,
        string temperature,
        string gesture)
    {
        Text = $"Student Details - {studentName}";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(470, 360);

        var heading = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 13F, FontStyle.Bold),
            Location = new Point(16, 14),
            Text = studentName,
        };

        var severityBadge = new Label
        {
            AutoSize = true,
            BackColor = severity == "Critical"
                ? Color.FromArgb(253, 224, 220)
                : severity == "Warning"
                    ? Color.FromArgb(255, 239, 214)
                    : severity == "Normal"
                        ? Color.FromArgb(223, 244, 231)
                        : Color.FromArgb(231, 236, 241),
            ForeColor = severity == "Critical"
                ? Color.FromArgb(163, 49, 36)
                : severity == "Warning"
                    ? Color.FromArgb(163, 98, 18)
                    : severity == "Normal"
                        ? Color.FromArgb(19, 109, 62)
                        : Color.FromArgb(95, 105, 115),
            Location = new Point(16, 56),
            Padding = new Padding(10, 6, 10, 6),
            Text = severity,
        };

        var details = new TableLayoutPanel
        {
            ColumnCount = 2,
            Location = new Point(16, 100),
            RowCount = 7,
            Size = new Size(438, 208),
        };
        details.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
        details.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        AddRow(details, 0, "Connection", connection);
        AddRow(details, 1, "Last Reading", lastSeen);
        AddRow(details, 2, "Heart Rate", heartRate);
        AddRow(details, 3, "Sweatness", sweatness);
        AddRow(details, 4, "Temperature", temperature);
        AddRow(details, 5, "Gesture", gesture);
        AddRow(details, 6, "Device ID", deviceId);

        var closeButton = new Button
        {
            DialogResult = DialogResult.OK,
            Location = new Point(362, 318),
            Size = new Size(92, 34),
            Text = "Close",
        };

        Controls.Add(heading);
        Controls.Add(severityBadge);
        Controls.Add(details);
        Controls.Add(closeButton);

        AcceptButton = closeButton;
        CancelButton = closeButton;
    }

    private static void AddRow(TableLayoutPanel table, int rowIndex, string labelText, string valueText)
    {
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var label = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(90, 101, 115),
            Margin = new Padding(0, 0, 0, 12),
            Text = labelText,
        };

        var value = new Label
        {
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 12),
            MaximumSize = new Size(284, 0),
            Text = valueText,
        };

        table.Controls.Add(label, 0, rowIndex);
        table.Controls.Add(value, 1, rowIndex);
    }
}
