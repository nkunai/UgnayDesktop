using System;
using System.Drawing;
using System.Windows.Forms;

namespace UgnayDesktop.Forms;

internal sealed class StudentAlertDialog : Form
{
    private readonly TextBox _messageTextBox;

    public string AlertMessage => _messageTextBox.Text.Trim();

    public StudentAlertDialog(string studentName, string initialMessage)
    {
        Text = $"Send Alert - {studentName}";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(540, 320);

        var helpLabel = new Label
        {
            AutoSize = true,
            Location = new Point(16, 16),
            Text = "Review the SMS message before sending it to the teacher phone.",
        };

        _messageTextBox = new TextBox
        {
            Location = new Point(16, 48),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Size = new Size(508, 208),
            TabIndex = 0,
            Text = initialMessage,
        };

        var cancelButton = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(346, 272),
            Size = new Size(84, 34),
            TabIndex = 2,
        };

        var sendButton = new Button
        {
            Text = "Send Alert",
            Location = new Point(436, 272),
            Size = new Size(88, 34),
            TabIndex = 1,
        };
        sendButton.Click += SendButton_Click;

        Controls.Add(helpLabel);
        Controls.Add(_messageTextBox);
        Controls.Add(cancelButton);
        Controls.Add(sendButton);

        AcceptButton = sendButton;
        CancelButton = cancelButton;
    }

    private void SendButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(AlertMessage))
        {
            MessageBox.Show("Alert message cannot be empty.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _messageTextBox.Focus();
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}
