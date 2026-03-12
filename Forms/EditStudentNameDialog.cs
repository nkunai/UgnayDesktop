using System;
using System.Drawing;
using System.Windows.Forms;

namespace UgnayDesktop.Forms;

internal sealed class EditStudentNameDialog : Form
{
    private readonly TextBox _fullNameTextBox;

    public string StudentFullName => _fullNameTextBox.Text.Trim();

    public EditStudentNameDialog(string currentFullName)
    {
        Text = "Edit Student Name";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(430, 160);

        var fullNameLabel = new Label
        {
            Text = "Student Name",
            AutoSize = true,
            Location = new Point(16, 18)
        };

        _fullNameTextBox = new TextBox
        {
            Location = new Point(16, 46),
            Size = new Size(396, 35),
            TabIndex = 0,
            Text = currentFullName?.Trim() ?? string.Empty
        };

        var cancelButton = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(236, 108),
            Size = new Size(86, 36),
            TabIndex = 2
        };

        var saveButton = new Button
        {
            Text = "Save",
            Location = new Point(326, 108),
            Size = new Size(86, 36),
            TabIndex = 1
        };
        saveButton.Click += SaveButton_Click;

        Controls.Add(fullNameLabel);
        Controls.Add(_fullNameTextBox);
        Controls.Add(cancelButton);
        Controls.Add(saveButton);

        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(StudentFullName))
        {
            MessageBox.Show("Student name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _fullNameTextBox.Focus();
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}
