using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace UgnayDesktop.Forms;

internal sealed class EditTeacherProfileDialog : Form
{
    private readonly TextBox _fullNameTextBox;
    private readonly TextBox _contactDigitsTextBox;

    public string TeacherFullName => _fullNameTextBox.Text.Trim();
    public string ContactDigits => _contactDigitsTextBox.Text.Trim();
    public string NormalizedPhoneNumber => $"+63{ContactDigits}";

    public EditTeacherProfileDialog(string fullName, string contactDigits)
    {
        Text = "Edit Profile";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(440, 230);

        var fullNameLabel = new Label
        {
            Text = "Full Name",
            AutoSize = true,
            Location = new Point(16, 18)
        };

        _fullNameTextBox = new TextBox
        {
            Location = new Point(16, 46),
            Size = new Size(406, 35),
            TabIndex = 0,
            Text = fullName?.Trim() ?? string.Empty
        };

        var contactLabel = new Label
        {
            Text = "Contact Number",
            AutoSize = true,
            Location = new Point(16, 92)
        };

        var prefixLabel = new Label
        {
            Text = "+63",
            AutoSize = true,
            Location = new Point(16, 126)
        };

        _contactDigitsTextBox = new TextBox
        {
            Location = new Point(62, 122),
            Size = new Size(180, 35),
            MaxLength = 10,
            TabIndex = 1,
            Text = NormalizeContactDigits(contactDigits)
        };
        _contactDigitsTextBox.KeyPress += ContactDigitsTextBox_KeyPress;

        var hintLabel = new Label
        {
            Text = "Use 10 digits starting with 9 (example: 9186764468).",
            AutoSize = true,
            ForeColor = Color.DimGray,
            Location = new Point(16, 162)
        };

        var cancelButton = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(244, 186),
            Size = new Size(86, 34),
            TabIndex = 3
        };

        var saveButton = new Button
        {
            Text = "Save",
            Location = new Point(336, 186),
            Size = new Size(86, 34),
            TabIndex = 2
        };
        saveButton.Click += SaveButton_Click;

        Controls.Add(fullNameLabel);
        Controls.Add(_fullNameTextBox);
        Controls.Add(contactLabel);
        Controls.Add(prefixLabel);
        Controls.Add(_contactDigitsTextBox);
        Controls.Add(hintLabel);
        Controls.Add(cancelButton);
        Controls.Add(saveButton);

        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    private static string NormalizeContactDigits(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var digits = new string(input.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("63", StringComparison.Ordinal) && digits.Length >= 12)
        {
            digits = digits.Substring(2);
        }
        else if (digits.StartsWith("0", StringComparison.Ordinal) && digits.Length >= 11)
        {
            digits = digits.Substring(1);
        }

        if (digits.Length > 10)
        {
            digits = digits.Substring(digits.Length - 10);
        }

        return digits;
    }

    private static bool IsValidContactDigits(string digits)
    {
        return digits.Length == 10 && digits[0] == '9' && digits.All(char.IsDigit);
    }

    private void ContactDigitsTextBox_KeyPress(object? sender, KeyPressEventArgs e)
    {
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
        {
            e.Handled = true;
        }
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TeacherFullName))
        {
            MessageBox.Show("Full name is required.", "Profile", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _fullNameTextBox.Focus();
            return;
        }

        var digits = ContactDigits;
        if (!IsValidContactDigits(digits))
        {
            MessageBox.Show("Contact number must be 10 digits starting with 9 after +63.", "Profile", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _contactDigitsTextBox.Focus();
            return;
        }

        var confirm = MessageBox.Show("Apply profile changes?", "Confirm Profile Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes)
        {
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}
