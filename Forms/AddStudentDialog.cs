using System;
using System.Drawing;
using System.Windows.Forms;

namespace UgnayDesktop.Forms;

internal sealed class AddStudentDialog : Form
{
    private readonly TextBox _fullNameTextBox;
    private readonly NumericUpDown _ageInput;
    private readonly ComboBox _sexComboBox;

    public string StudentFullName => _fullNameTextBox.Text.Trim();
    public int StudentAge => (int)_ageInput.Value;
    public string? StudentSex => _sexComboBox.SelectedItem?.ToString();

    public AddStudentDialog()
    {
        Text = "Add Student";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(420, 250);

        var fullNameLabel = new Label
        {
            Text = "Full Name",
            AutoSize = true,
            Location = new Point(16, 18)
        };

        _fullNameTextBox = new TextBox
        {
            Location = new Point(16, 46),
            Size = new Size(386, 35),
            TabIndex = 0
        };

        var ageLabel = new Label
        {
            Text = "Age",
            AutoSize = true,
            Location = new Point(16, 92)
        };

        _ageInput = new NumericUpDown
        {
            Location = new Point(16, 120),
            Size = new Size(120, 35),
            Minimum = 1,
            Maximum = 120,
            Value = 7,
            TabIndex = 1
        };

        var sexLabel = new Label
        {
            Text = "Sex",
            AutoSize = true,
            Location = new Point(154, 92)
        };

        _sexComboBox = new ComboBox
        {
            Location = new Point(154, 118),
            Size = new Size(160, 38),
            DropDownStyle = ComboBoxStyle.DropDownList,
            TabIndex = 2
        };
        _sexComboBox.Items.AddRange(new object[] { "Male", "Female" });

        var cancelButton = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(218, 190),
            Size = new Size(88, 40),
            TabIndex = 4
        };

        var addButton = new Button
        {
            Text = "Add Student",
            Location = new Point(314, 190),
            Size = new Size(88, 40),
            TabIndex = 3
        };
        addButton.Click += AddButton_Click;

        Controls.Add(fullNameLabel);
        Controls.Add(_fullNameTextBox);
        Controls.Add(ageLabel);
        Controls.Add(_ageInput);
        Controls.Add(sexLabel);
        Controls.Add(_sexComboBox);
        Controls.Add(cancelButton);
        Controls.Add(addButton);

        AcceptButton = addButton;
        CancelButton = cancelButton;
    }

    private void AddButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(StudentFullName))
        {
            MessageBox.Show("Full name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _fullNameTextBox.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(StudentSex))
        {
            MessageBox.Show("Please select sex.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _sexComboBox.DroppedDown = true;
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}
