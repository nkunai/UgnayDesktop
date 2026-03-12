using System.ComponentModel;
using System.Drawing;

namespace UgnayDesktop.Controls;

public partial class StudentCardControl : UserControl
{
    public event EventHandler? CardClicked;

    public StudentCardControl()
    {
        InitializeComponent();
        WireCardClickForwarding(this);
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public string StudentName
    {
        get => lblHeading.Text;
        set => lblHeading.Text = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string DeviceIdValue
    {
        get => lblDeviceIdValue.Text;
        set => lblDeviceIdValue.Text = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string GestureValue
    {
        get => lblGestureValue.Text;
        set => lblGestureValue.Text = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string HeartRateValue
    {
        get => lblHeartRateValue.Text;
        set => lblHeartRateValue.Text = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string SweatnessValue
    {
        get => lblSweatnessValue.Text;
        set => lblSweatnessValue.Text = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string TemperatureValue
    {
        get => lblTemperatureValue.Text;
        set => lblTemperatureValue.Text = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string ConnectionValue
    {
        get => lblConnectionValue.Text;
        set => lblConnectionValue.Text = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color ConnectionValueColor
    {
        get => lblConnectionValue.ForeColor;
        set => lblConnectionValue.ForeColor = value;
    }

    private void WireCardClickForwarding(Control root)
    {
        root.Click += ForwardCardClick;
        root.Cursor = Cursors.Hand;

        foreach (Control child in root.Controls)
        {
            WireCardClickForwarding(child);
        }
    }

    private void ForwardCardClick(object? sender, EventArgs e)
    {
        CardClicked?.Invoke(this, EventArgs.Empty);
    }
}
